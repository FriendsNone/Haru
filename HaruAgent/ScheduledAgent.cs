using HaruCore;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
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
            if (!HaruSettings.BackgroundUpdateEnabled)
                return false;

            var targets = GetTargets();
            if (targets.Count == 0)
                return false;

            var temperatureUnit = HaruSettings.TemperatureUnit;
            var windSpeedUnit = HaruSettings.WindSpeedUnit;
            var precipitationUnit = HaruSettings.PrecipitationUnit;
            var liveTile = HaruSettings.LiveTileEnabled;
            var mono = HaruSettings.MonochromeTileEnabled;
            var notify = HaruSettings.NotificationEnabled;
            var pending = targets.Count;

            foreach (var target in targets)
            {
                client.GetForecast(target.Place.Latitude, target.Place.Longitude, temperatureUnit, windSpeedUnit, precipitationUnit, (forecast, error) =>
                {
                    try
                    {
                        if (forecast != null)
                            ApplyForecast(target, forecast, error == null, liveTile, mono);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Periodic task failed for " + target.Place.Name + ": " + ex);
                    }
                    finally
                    {
                        if (Interlocked.Decrement(ref pending) == 0)
                            FinishUpdate(task, targets, temperatureUnit, notify);
                    }
                });
            }
            return true;
        }

        private static List<UpdateTarget> GetTargets()
        {
            var targets = new List<UpdateTarget>();

            var home = HaruSettings.HomePlace;
            if (home != null)
                targets.Add(new UpdateTarget { Place = home, IsHome = true });

            foreach (var tile in ShellTile.ActiveTiles)
            {
                var place = TileHelper.GetPlace(tile);
                if (place == null) continue;

                var target = targets.FirstOrDefault(t => t.Place.IsSameAs(place));
                if (target == null)
                {
                    target = new UpdateTarget { Place = place };
                    targets.Add(target);
                }
                target.Tiles.Add(tile);
            }

            return targets;
        }

        private static void ApplyForecast(UpdateTarget target, ForecastResponse forecast, bool isFresh, bool liveTile, bool mono)
        {
            var current = forecast.ToCurrentRecord();
            var currentData = forecast.Current;

            if (liveTile)
            {
                if (target.IsHome)
                    TileHelper.UpdateTile(TileHelper.PrimaryTile, target.Place.Name, current, mono);

                foreach (var tile in target.Tiles)
                    TileHelper.UpdateTile(tile, target.Place.Name, current, mono);
            }

            if (isFresh && currentData.Temperature.HasValue && currentData.WeatherCode.HasValue)
            {
                target.Notification = new NotificationCandidate
                {
                    Place = target.Place,
                    Current = current,
                    Temperature = currentData.Temperature.Value,
                    WeatherCode = currentData.WeatherCode.Value,
                    NavigationUri = target.IsHome
                        ? new Uri("/Views/MainPage.xaml", UriKind.Relative)
                        : TileHelper.NavigationUriFor(target.Place)
                };
            }
        }

        private void FinishUpdate(ScheduledTask task, List<UpdateTarget> targets, string temperatureUnit, bool notify)
        {
            try
            {
                if (notify)
                    NotificationHelper.MaybeNotify(
                        targets.Where(t => t.Notification != null).Select(t => t.Notification).ToList(),
                        targets.Select(t => t.Place.Key),
                        temperatureUnit
                    );

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
        }

        private void Complete()
        {
            if (Interlocked.Exchange(ref completed, 1) == 0)
                NotifyComplete();
        }

        private class UpdateTarget
        {
            public Place Place;
            public bool IsHome;
            public readonly List<ShellTile> Tiles = new List<ShellTile>();
            public NotificationCandidate Notification;
        }
    }
}