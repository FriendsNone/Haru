using HaruCore;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading;
using System.Windows;


namespace HaruAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        private static volatile bool _classInitialized;
        private readonly IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
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

            if (!SettingsHelper.GetBool(settings, "BackgroundUpdateEnable", true))
                return false;

            string[] requiredKeys =
            {
                "Location", "Latitude", "Longitude",
                "TemperatureUnit", "WindSpeedUnit", "PrecipitationUnit"
            };

            if (requiredKeys.Any(key => !settings.Contains(key)))
                return false;

            var location = (string)settings["Location"];
            var latitude = (double)settings["Latitude"];
            var longitude = (double)settings["Longitude"];
            var temperatureUnit = (string)settings["TemperatureUnit"];
            var windSpeedUnit = (string)settings["WindSpeedUnit"];
            var precipitationUnit = (string)settings["PrecipitationUnit"];

            client.GetForecast(latitude, longitude, temperatureUnit, windSpeedUnit, precipitationUnit, (forecast, error) =>
            {
                try
                {
                    if (forecast != null)
                        ApplyForecast(forecast, location, temperatureUnit);

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

        private void ApplyForecast(ForecastResponse forecast, string location, string temperatureUnit)
        {
            var current = forecast.ToCurrentRecord();
            var currentData = forecast.Current;

            if (SettingsHelper.GetBool(settings, "LiveTileEnable", true))
            {
                TileHelper.UpdateTile(
                    location,
                    current.Temperature,
                    current.WeatherDescription,
                    current.WeatherIcon,
                    current.WeatherTile,
                    UnitHelper.FormatObservationTime(current.ObservedUtc),
                    SettingsHelper.GetBool(settings, "MonochromeTileEnable", false)
                );
            }

            if (SettingsHelper.GetBool(settings, "NotificationEnable", true))
            {
                NotificationHelper.MaybeNotify(
                    location,
                    current,
                    currentData.Temperature,
                    currentData.WeatherCode,
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