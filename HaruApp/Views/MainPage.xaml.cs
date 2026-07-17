using HaruApp.Helpers;
using HaruApp.Resources;
using HaruApp.ViewModels;
using HaruCore;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;

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
            BuildApplicationBar();
            DataContext = vm;
            timer = ProgressHelper.CreateProgressTimer(progressIndicator);
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            SystemTray.ProgressIndicator = progressIndicator;

            if (!settings.Contains("FirstTimeLocation"))
            {
                PromptHelper.ShowPrompt(
                    AppResources.PromptNoLocationTitle,
                    AppResources.PromptNoLocationFirstTime,
                    AppResources.PromptYes,
                    AppResources.PromptLater,
                    () =>
                    {
                        settings["FirstTimeLocation"] = true;
                        settings.Save();
                        NavigationService.Navigate(new Uri("/Views/SearchPage.xaml", UriKind.Relative));
                    },
                    () =>
                    {
                        settings["FirstTimeLocation"] = true;
                        settings.Save();
                    },
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

            // Arrived here via a "?refresh=true" redirect from Search or Settings. Drop the
            // intermediate page(s) from the back stack (up to two) so pressing Back exits the
            // app to the Start screen instead of returning to those transient pages.
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
                MainPivot.Title = lastLocation.ToUpper();
                if (MainPivot.SelectedIndex != 0) MainPivot.SelectedIndex = 0;
                FetchForecast();
            }
        }

        private void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ApplicationBar != null)
            {
                ApplicationBar.Mode = MainPivot.SelectedIndex == 0 ? ApplicationBarMode.Default : ApplicationBarMode.Minimized;
                ScrollToTop(HoursListBox);
                ScrollToTop(DaysListBox);
            }
        }

        private static void ScrollToTop(ListBox list)
        {
            if (list.Items.Count > 0)
                list.ScrollIntoView(list.Items[0]);
        }

        private void SearchApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/SearchPage.xaml", UriKind.Relative));
        }

        private void RefreshApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            if (!HasLocationSettings())
                PromptHelper.ShowPrompt(
                    AppResources.PromptNoLocationTitle,
                    AppResources.PromptNoLocationRefresh,
                    AppResources.PromptYes,
                    AppResources.PromptLater,
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

            ProgressHelper.ShowProgress(progressIndicator, AppResources.ProgressFetchingForecast);

            client.GetForecast(latitude, longitude, temperatureUnit, windSpeedUnit, precipitationUnit, (forecast, error) =>
            {
                if (forecast == null)
                {
                    ProgressHelper.ShowProgress(progressIndicator, AppResources.ProgressError, true, timer);
                    return;
                }

                vm.Current = forecast.ToCurrentRecord();
                vm.Hourly = forecast.ToHourlyRecords();
                vm.Daily = forecast.ToDailyRecords();
                NowScrollViewer.Visibility = Visibility.Visible;
                UpdateTile(vm.Current);

                if (error != null)
                    ProgressHelper.ShowProgress(progressIndicator, AppResources.ProgressShowingLastUpdate, true, timer);
                else
                    ProgressHelper.HideProgress(progressIndicator);
            });
        }

        private void UpdateTile(CurrentRecord cr)
        {
            var showWeatherTile = SettingsHelper.GetBool(settings, "BackgroundUpdateEnable", true)
                && SettingsHelper.GetBool(settings, "LiveTileEnable", true);

            if (showWeatherTile)
                TileHelper.UpdateTile(
                    (string)settings["Location"],
                    cr.Temperature,
                    cr.WeatherDescription,
                    cr.WeatherIcon,
                    cr.WeatherTile,
                    DateTime.Now.ToString("t"),
                    SettingsHelper.GetBool(settings, "MonochromeTileEnable", false)
                );
            else
                TileHelper.ResetTile();

            StartPeriodicAgent();
        }

        private void StartPeriodicAgent()
        {
            var oldTask = ScheduledActionService.Find(TASK_NAME);
            if (oldTask != null)
                ScheduledActionService.Remove(TASK_NAME);

            var backgroundUpdateEnabled = SettingsHelper.GetBool(settings, "BackgroundUpdateEnable", true);
            var liveTileEnabled = SettingsHelper.GetBool(settings, "LiveTileEnable", true);
            var notificationEnabled = SettingsHelper.GetBool(settings, "NotificationEnable", true);

            if (!backgroundUpdateEnabled || (!liveTileEnabled && !notificationEnabled))
                return;

            task = new PeriodicTask(TASK_NAME)
            {
                Description = "Updates the live tile and weather alerts with the latest forecast.",
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
                    MessageBox.Show(AppResources.BackgroundAgentDisabled);
            }
            catch (SchedulerServiceException) { }
        }

        private bool HasLocationSettings()
        {
            return settings.Contains("Location") && settings.Contains("Latitude") && settings.Contains("Longitude");
        }

        private void BuildApplicationBar()
        {
            ApplicationBar = new ApplicationBar();

            ApplicationBarIconButton searchButton = new ApplicationBarIconButton();
            searchButton.IconUri = new Uri("/Assets/AppBar/appbar.feature.search.rest.png", UriKind.Relative);
            searchButton.Text = AppResources.AppBarSearch;
            searchButton.Click += SearchApplicationBarIconButton_Click;
            ApplicationBar.Buttons.Add(searchButton);

            ApplicationBarIconButton refreshButton = new ApplicationBarIconButton();
            refreshButton.IconUri = new Uri("/Assets/AppBar/appbar.refresh.rest.png", UriKind.Relative);
            refreshButton.Text = AppResources.AppBarRefresh;
            refreshButton.Click += RefreshApplicationBarIconButton_Click;
            ApplicationBar.Buttons.Add(refreshButton);

            ApplicationBarMenuItem settingsItem = new ApplicationBarMenuItem(AppResources.AppBarSettings);
            settingsItem.Click += SettingsApplicationBarMenuItem_Click;
            ApplicationBar.MenuItems.Add(settingsItem);

            ApplicationBarMenuItem aboutItem = new ApplicationBarMenuItem(AppResources.AppBarAbout);
            aboutItem.Click += AboutApplicationBarMenuItem_Click;
            ApplicationBar.MenuItems.Add(aboutItem);
        }
    }
}