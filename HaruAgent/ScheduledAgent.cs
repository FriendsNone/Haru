using System.Windows;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using System.Linq;
using System;
using HaruCore;

namespace HaruAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        private static volatile bool _classInitialized;
        private IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        private OpenMeteoClient client;

        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        public ScheduledAgent()
        {
            if (!_classInitialized)
            {
                _classInitialized = true;
                client = new OpenMeteoClient();
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
            string location = (string)settings["Location"];
            double latitude = (double)settings["Latitude"];
            double longitude = (double)settings["Longitude"];

            string temperatureUnit = (string)settings["TemperatureUnit"];
            string windSpeedUnit = (string)settings["WindSpeedUnit"];
            string precipitationUnit = (string)settings["PrecipitationUnit"];

            ShellTile tile = ShellTile.ActiveTiles.First();
            if (tile != null)
            {
                client.GetForecast(latitude, longitude, temperatureUnit, windSpeedUnit, precipitationUnit, (forecast, ferr) =>
                {
                    if (forecast == null)
                    {
                        NotifyComplete();
                        return;
                    }

                    CurrentRecord cr = forecast.ToCurrentRecord();

                    StandardTileData data = new StandardTileData()
                    {
                        Title = location,
#if DEBUG
                        Count = new Random().Next(99),
#endif
                        BackgroundImage = new Uri(cr.WeatherTile, UriKind.Relative),
                        BackTitle = DateTime.Now.ToString("t"),
                        BackContent = string.Format("{0}\n{1}",
                            cr.Temperature,
                            cr.WeatherDescription),
                    };

                    tile.Update(data);

#if DEBUG
                    ScheduledActionService.LaunchForTest(task.Name, TimeSpan.FromSeconds(60));
                    System.Diagnostics.Debug.WriteLine("Periodic task is started again: " + task.Name);
#endif

                    NotifyComplete();
                });
            }
            else
            {
                NotifyComplete();
            }
        }
    }
}