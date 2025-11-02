using System;
using System.Linq;
using System.Windows;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using HaruCore;


namespace HaruAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        private static volatile bool _classInitialized;
        private readonly IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        private readonly OpenMeteoClient client = new OpenMeteoClient();

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
            var tile = ShellTile.ActiveTiles.FirstOrDefault();
            if (tile == null)
            {
                NotifyComplete();
                return;
            }

            var location = (string)settings["Location"];
            var latitude = (double)settings["Latitude"];
            var longitude = (double)settings["Longitude"];
            var temperatureUnit = (string)settings["TemperatureUnit"];
            var windSpeedUnit = (string)settings["WindSpeedUnit"];
            var precipitationUnit = (string)settings["PrecipitationUnit"];

            client.GetForecast(latitude, longitude, temperatureUnit, windSpeedUnit, precipitationUnit, (forecast, error) =>
            {
                if (forecast != null)
                {
                    var current = forecast.ToCurrentRecord();

                    TileHelper.UpdateTile(
                        location,
                        current.Temperature,
                        current.WeatherDescription,
                        current.WeatherIcon,
                        current.WeatherTile,
                        error != null ? current.Time : DateTime.Now.ToString("t")
                    );

#if DEBUG
                    ScheduledActionService.LaunchForTest(task.Name, TimeSpan.FromSeconds(60));
                    System.Diagnostics.Debug.WriteLine("Periodic task is started again: " + task.Name);
#endif
                }

                NotifyComplete();
            });
        }
    }
}