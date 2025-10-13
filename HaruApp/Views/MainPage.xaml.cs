using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.IO.IsolatedStorage;
using System.Windows.Threading;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Scheduler;
using HaruCore;
using HaruApp.ViewModels;
using HaruApp.Helpers;

namespace HaruApp.Views
{
    public partial class MainPage : PhoneApplicationPage
    {
        private const string TASK_NAME = "HaruAgent";

        private readonly IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        private readonly OpenMeteoClient client = new OpenMeteoClient();
        private readonly ProgressIndicator progressIndicator = new ProgressIndicator();
        private readonly ForecastViewModel vm = new ForecastViewModel();
        private readonly DispatcherTimer timer;
        private string lastLocation;
        private PeriodicTask task;

        public MainPage()
        {
            InitializeComponent();
            DataContext = vm;
            timer = ProgressHelper.CreateProgressTimer(progressIndicator);
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            SystemTray.ProgressIndicator = progressIndicator;

            if (!HasLocationSettings())
                GuideCanvas.Visibility = Visibility.Visible;

            if (!settings.Contains("FirstTimeLocation"))
            {
                PromptHelper.ShowPrompt(
                    "No location set",
                    "If this is your first time using Haru, you need to set a location before fetching the forecast. Do you want to set it now?",
                    "yes",
                    "later",
                    () =>
                    {
                        settings["FirstTimeLocation"] = true;
                        settings.Save();
                        NavigationService.Navigate(new Uri("/Views/SearchPage.xaml", UriKind.Relative));
                    },
                    null,
                    () =>
                    {
                        settings["FirstTimeLocation"] = true;
                        settings.Save();
                    });
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (NavigationContext.QueryString.ContainsKey("refresh"))
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
                lastLocation = settings["Location"] as string;
                GuideCanvas.Visibility = Visibility.Collapsed;
                MainPivot.Title = lastLocation.ToUpper();
                if (MainPivot.SelectedIndex != 0) MainPivot.SelectedIndex = 0;
                FetchForecast();
            }
        }

        private void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ApplicationBar != null)
                ApplicationBar.Mode = MainPivot.SelectedIndex == 0 ? ApplicationBarMode.Default : ApplicationBarMode.Minimized;
        }

        private void SearchApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/SearchPage.xaml", UriKind.Relative));
        }

        private void RefreshApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            if (!HasLocationSettings())
                PromptHelper.ShowPrompt(
                    "No location set",
                    "You need to set a location before refreshing the forecast. Do you want to set it now?",
                    "yes",
                    "later",
                    () => NavigationService.Navigate(new Uri("/Views/SearchPage.xaml", UriKind.Relative)));
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
            var latitude = (double)settings["Latitude"];
            var longitude = (double)settings["Longitude"];
            var temperatureUnit = (string)settings["TemperatureUnit"];
            var windSpeedUnit = (string)settings["WindSpeedUnit"];
            var precipitationUnit = (string)settings["PrecipitationUnit"];

            ProgressHelper.ShowProgress(progressIndicator, "Fetching forecast...");

            client.GetForecast(latitude, longitude, temperatureUnit, windSpeedUnit, precipitationUnit, (forecast, error) =>
            {
                if (forecast == null)
                {
                    ProgressHelper.ShowProgress(progressIndicator, "Something went wrong. Try again later.", true, timer);
                    return;
                }

                vm.Current = forecast.ToCurrentRecord();
                vm.Hourly = forecast.ToHourlyRecords();
                vm.Daily = forecast.ToDailyRecords();
                NowScrollViewer.Visibility = Visibility.Visible;
                UpdateTile(vm.Current);

                if (error != null)
                    ProgressHelper.ShowProgress(progressIndicator, "Something went wrong. Showing last update.", true, timer);
                else
                    ProgressHelper.HideProgress(progressIndicator);
            });
        }

        private void UpdateTile(CurrentRecord cr)
        {
            var tile = ShellTile.ActiveTiles.FirstOrDefault();
            if (tile != null)
            {
                tile.Update(new StandardTileData
                {
                    Title = (string)settings["Location"],
                    Count = 0,
                    BackgroundImage = new Uri(cr.WeatherTile, UriKind.Relative),
                    BackTitle = DateTime.Now.ToString("t"),
                    BackContent = string.Format("{0}\n{1}", cr.Temperature, cr.WeatherDescription)
                });
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

        private bool HasLocationSettings()
        {
            return settings.Contains("Location") && settings.Contains("Latitude") && settings.Contains("Longitude");
        }
    }
}