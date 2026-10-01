using HaruCore;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System;
using System.Linq;
using System.Threading;
using System.Windows;


namespace HaruAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        private static volatile bool _classInitialized;
        private readonly OpenMeteoClient client = new OpenMeteoClient { RequestTimeout = TimeSpan.FromSeconds(15) };
        private int completed;

        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        public ScheduledAgent()
        {
            if (!_classInitialized)
            {
                _classInitialized = true;
                // Subscribe to the managed exception handler
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    Application.Current.UnhandledException += ScheduledAgent_UnhandledException;
                });
            }
        }

        /// Code to execute on Unhandled Exceptions
        private void ScheduledAgent_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected override void OnInvoke(ScheduledTask task)
        {
            try
            {
                if (!StartForecastUpdate(task))
                    Complete();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Periodic task failed to start: " + ex);
                Complete();
            }
        }

        private bool StartForecastUpdate(ScheduledTask task)
        {
            var tile = ShellTile.ActiveTiles.FirstOrDefault();
            if (tile == null)
                return false;

            if (!HaruSettings.BackgroundUpdateEnabled || !HaruSettings.HasLocation)
                return false;

            var location = HaruSettings.Location;
            var latitude = HaruSettings.Latitude;
            var longitude = HaruSettings.Longitude;
            var temperatureUnit = HaruSettings.TemperatureUnit;
            var windSpeedUnit = HaruSettings.WindSpeedUnit;
            var precipitationUnit = HaruSettings.PrecipitationUnit;

            client.GetForecast(latitude, longitude, temperatureUnit, windSpeedUnit, precipitationUnit, (forecast, error) =>
            {
                try
                {
                    if (forecast != null)
                        ApplyForecast(forecast, location, temperatureUnit, error == null);

#if DEBUG
                    ScheduledActionService.LaunchForTest(task.Name, TimeSpan.FromSeconds(60));
                    System.Diagnostics.Debug.WriteLine("Periodic task is started again: " + task.Name);
#endif
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Periodic task failed: " + ex);
                }
                finally
                {
                    Complete();
                }
            });
            return true;
        }

        private void ApplyForecast(ForecastResponse forecast, string location, string temperatureUnit, bool isFresh)
        {
            var current = forecast.ToCurrentRecord();
            var currentData = forecast.Current;

            if (HaruSettings.LiveTileEnabled)
            {
                TileHelper.UpdateTile(
                    location,
                    current.Temperature,
                    current.WeatherDescription,
                    current.WeatherIcon,
                    current.WeatherTile,
                    UnitHelper.FormatObservationTime(current.ObservedUtc),
                    HaruSettings.MonochromeTileEnabled
                );
            }

            if (isFresh
                && currentData.Temperature.HasValue && currentData.WeatherCode.HasValue
                && HaruSettings.NotificationEnabled)
            {
                NotificationHelper.MaybeNotify(
                    location,
                    current,
                    currentData.Temperature.Value,
                    currentData.WeatherCode.Value,
                    temperatureUnit
                );
            }
        }

        private void Complete()
        {
            if (Interlocked.Exchange(ref completed, 1) == 0)
                NotifyComplete();
        }
    }
}