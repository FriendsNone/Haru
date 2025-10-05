using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using System.Windows.Navigation;
using HaruCore;
using System.Windows.Media.Imaging;
using HaruApp.ViewModels;
using System.Windows.Threading;
using Microsoft.Phone.Scheduler;

namespace HaruApp.Views
{
    public partial class MainPage : PhoneApplicationPage
    {
        private const string TASK_NAME = "HaruAgent";

        private IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        private OpenMeteoClient client;
        private PeriodicTask task;
        private ProgressIndicator progressIndicator;
        private string lastLocation;
        private ForecastViewModel vm = new ForecastViewModel();
        private DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };

        public MainPage()
        {
            InitializeComponent();
            this.DataContext = vm;
            client = new OpenMeteoClient();
            progressIndicator = new ProgressIndicator();

            timer.Tick += (s, args) =>
            {
                timer.Stop();
                progressIndicator.IsVisible = false;
                return;
            };
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            SystemTray.ProgressIndicator = progressIndicator;

            if (!settings.Contains("Location") || !settings.Contains("Latitude") || !settings.Contains("Longitude"))
                GuideCanvas.Visibility = Visibility.Visible;

            if (!settings.Contains("FirstTimeLocation"))
            {
                CustomMessageBox messageBox = new CustomMessageBox()
                {
                    Caption = "No location set",
                    Message = "If this is your first time using Haru, you need to set a location before fetching the forecast. Do you want to set it now?",
                    LeftButtonContent = "yes",
                    RightButtonContent = "later"
                };

                messageBox.Dismissed += (s1, e1) =>
                {
                    switch (e1.Result)
                    {
                        case CustomMessageBoxResult.LeftButton:
                            settings["FirstTimeLocation"] = true;
                            settings.Save();
                            NavigationService.Navigate(new Uri("/Views/SearchPage.xaml", UriKind.Relative));
                            break;
                        case CustomMessageBoxResult.RightButton:
                        case CustomMessageBoxResult.None:
                        default:
                            settings["FirstTimeLocation"] = true;
                            settings.Save();
                            break;
                    }
                };

                messageBox.Show();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string refresh;
            if (NavigationContext.QueryString.TryGetValue("refresh", out refresh))
            {
                if (NavigationService.CanGoBack)
                {
                    NavigationService.RemoveBackEntry();
                    if (NavigationService.CanGoBack)
                        NavigationService.RemoveBackEntry();
                }
            }

            if (settings.Contains("Location") && settings["Location"] as string != lastLocation)
            {
                string location = settings["Location"] as string;
                lastLocation = location;

                GuideCanvas.Visibility = Visibility.Collapsed;
                MainPivot.Title = location.ToUpper();

                FetchForecast();
            }
        }

        private void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ApplicationBar != null)
            {
                ApplicationBar.Mode = MainPivot.SelectedIndex == 0
                    ? ApplicationBarMode.Default
                    : ApplicationBarMode.Minimized;
            }
        }

        private void SearchApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/SearchPage.xaml", UriKind.Relative));
        }

        private void RefreshApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            if (!settings.Contains("Location") || !settings.Contains("Latitude") || !settings.Contains("Longitude"))
            {
                CustomMessageBox messageBox = new CustomMessageBox()
                {
                    Caption = "No location set",
                    Message = "You need to set a location before refreshing the forecast. Do you want to set it now?",
                    LeftButtonContent = "yes",
                    RightButtonContent = "later"
                };

                messageBox.Dismissed += (s1, e1) =>
                {
                    switch (e1.Result)
                    {
                        case CustomMessageBoxResult.LeftButton:
                            NavigationService.Navigate(new Uri("/Views/SearchPage.xaml", UriKind.Relative));
                            break;
                        case CustomMessageBoxResult.RightButton:
                        case CustomMessageBoxResult.None:
                        default:
                            break;
                    }
                };

                messageBox.Show();
            }
            else
                FetchForecast();
        }

        private void SettingsApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/SettingsPage.xaml", UriKind.Relative));
        }

        private void AboutApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/AboutPage.xaml", UriKind.Relative));
        }

        private void FetchForecast()
        {
            double latitude = (double)settings["Latitude"];
            double longitude = (double)settings["Longitude"];

            string temperatureUnit = (string)settings["TemperatureUnit"];
            string windSpeedUnit = (string)settings["WindSpeedUnit"];
            string precipitationUnit = (string)settings["PrecipitationUnit"];

            progressIndicator.IsIndeterminate = true;
            progressIndicator.Text = "Fetching forecast...";
            progressIndicator.IsVisible = true;

            client.GetForecast(latitude, longitude, temperatureUnit, windSpeedUnit, precipitationUnit, (forecast, ferr) =>
            {
                if (forecast == null)
                {
                    progressIndicator.IsIndeterminate = false;
                    progressIndicator.Text = "Something went wrong. Try again later.";
                    timer.Start();
                    return;
                }

                vm.Current = forecast.ToCurrentRecord();
                vm.Hourly = forecast.ToHourlyRecords();
                vm.Daily = forecast.ToDailyRecords();

                NowScrollViewer.Visibility = Visibility.Visible;
                UpdateTile(vm.Current);

                if (ferr != null)
                {
                    progressIndicator.IsIndeterminate = false;
                    progressIndicator.Text = "Something went wrong. Showing last update.";
                    timer.Start();
                }
                else
                {
                    progressIndicator.IsVisible = false;
                }
            });
        }

        private void UpdateTile(CurrentRecord cr)
        {
            ShellTile tile = ShellTile.ActiveTiles.FirstOrDefault();
            if (tile != null)
            {
                tile.Update(
                    new StandardTileData()
                    {
                        Title = (string)settings["Location"],
                        Count = 0,
                        BackgroundImage = new Uri(cr.WeatherTile, UriKind.Relative),
                        BackTitle = DateTime.Now.ToString("t"),
                        BackContent = string.Format("{0}\n{1}",
                            cr.Temperature,
                            cr.WeatherDescription),
                    }
                );

                StartPeriodicAgent();
            }

        }

        private void StartPeriodicAgent()
        {
            var oldTask = ScheduledActionService.Find(TASK_NAME);
            if (oldTask != null)
                ScheduledActionService.Remove(TASK_NAME);

            if (!(bool?)settings["BackgroundUpdateEnable"] ?? true)
                return;

            task = new PeriodicTask(TASK_NAME)
            {
                Description = "Updates the live tile with the latest forecast.",
                ExpirationTime = DateTime.Now.AddDays(14)
            };

            try
            {
                ScheduledActionService.Add(task);
#if DEBUG
                ScheduledActionService.LaunchForTest(TASK_NAME, TimeSpan.FromSeconds(60));
                System.Diagnostics.Debug.WriteLine("Periodic task is started: " + TASK_NAME);
#endif
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("BNS Error: The action is disabled"))
                    MessageBox.Show("Background agents for this application have been disabled by the user.");
            }
            catch (SchedulerServiceException) { }
        }
    }
}