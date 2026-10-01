using HaruApp.Helpers;
using HaruApp.Resources;
using HaruApp.ViewModels;
using HaruCore;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace HaruApp.Views
{
    public partial class MainPage : PhoneApplicationPage
    {
        private const string TASK_NAME = "HaruAgent";
        private static readonly TimeSpan RefetchAfter = TimeSpan.FromMinutes(30);

        private readonly OpenMeteoClient client = new OpenMeteoClient();
        private readonly ProgressIndicator progressIndicator = new ProgressIndicator();
        private readonly ForecastViewModel vm = new ForecastViewModel();
        private readonly DispatcherTimer timer;
        private readonly DispatcherTimer forecastTimeTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
        private string lastLocation;
        private bool isFetching;
        private PeriodicTask task;

        public MainPage()
        {
            InitializeComponent();
            BuildApplicationBar();
            DataContext = vm;
            timer = ProgressHelper.CreateProgressTimer(progressIndicator);
            forecastTimeTimer.Tick += (s, e) => vm.RefreshForecastTime();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            SystemTray.ProgressIndicator = progressIndicator;

            if (!HaruSettings.FirstTimeLocationShown)
            {
                PromptHelper.ShowPrompt(
                    AppResources.PromptNoLocationTitle,
                    AppResources.PromptNoLocationFirstTime,
                    AppResources.PromptYes,
                    AppResources.PromptLater,
                    () =>
                    {
                        HaruSettings.FirstTimeLocationShown = true;
                        HaruSettings.Save();
                        NavigationService.Navigate(new Uri("/Views/SearchPage.xaml", UriKind.Relative));
                    },
                    () =>
                    {
                        HaruSettings.FirstTimeLocationShown = true;
                        HaruSettings.Save();
                    },
                    () =>
                    {
                        HaruSettings.FirstTimeLocationShown = true;
                        HaruSettings.Save();
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

            if (HaruSettings.HasLocation && HaruSettings.Location != lastLocation)
            {
                lastLocation = HaruSettings.Location;
                MainPivot.Title = lastLocation.ToUpper();
                if (MainPivot.SelectedIndex != 0) MainPivot.SelectedIndex = 0;
                FetchForecast();
            }
            else if (!isFetching && vm.Current != null && DateTime.UtcNow - vm.Current.ObservedUtc > RefetchAfter)
            {
                FetchForecast();
            }

            vm.RefreshForecastTime();
            forecastTimeTimer.Start();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            forecastTimeTimer.Stop();
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
            if (!HaruSettings.HasLocation)
                PromptHelper.ShowPrompt(
                    AppResources.PromptNoLocationTitle,
                    AppResources.PromptNoLocationRefresh,
                    AppResources.PromptYes,
                    AppResources.PromptLater,
                    () => NavigationService.Navigate(new Uri("/Views/SearchPage.xaml", UriKind.Relative)));
            else if (!isFetching)
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
            var latitude = HaruSettings.Latitude;
            var longitude = HaruSettings.Longitude;
            var temperatureUnit = HaruSettings.TemperatureUnit;
            var windSpeedUnit = HaruSettings.WindSpeedUnit;
            var precipitationUnit = HaruSettings.PrecipitationUnit;

            isFetching = true;
            ProgressHelper.ShowProgress(progressIndicator, AppResources.ProgressFetchingForecast, timer: timer);

            client.GetForecast(latitude, longitude, temperatureUnit, windSpeedUnit, precipitationUnit, (forecast, error) =>
            {
                isFetching = false;

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
                    ProgressHelper.HideProgress(progressIndicator, timer);
            });
        }

        private void UpdateTile(CurrentRecord cr)
        {
            if (HaruSettings.BackgroundUpdateEnabled && HaruSettings.LiveTileEnabled)
                TileHelper.UpdateTile(
                    HaruSettings.Location,
                    cr.Temperature,
                    cr.WeatherDescription,
                    cr.WeatherIcon,
                    cr.WeatherTile,
                    UnitHelper.FormatObservationTime(cr.ObservedUtc),
                    HaruSettings.MonochromeTileEnabled
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

            if (!HaruSettings.BackgroundUpdateEnabled
                || (!HaruSettings.LiveTileEnabled && !HaruSettings.NotificationEnabled))
                return;

            task = new PeriodicTask(TASK_NAME)
            {
                Description = "Updates the live tile and weather alerts with the latest forecast.",
                ExpirationTime = DateTime.Now.AddDays(14)
            };

            try
            {
                ScheduledActionService.Add(task);

                if (HaruSettings.BackgroundAgentDisabledShown)
                {
                    HaruSettings.BackgroundAgentDisabledShown = false;
                    HaruSettings.Save();
                }
#if DEBUG
                ScheduledActionService.LaunchForTest(TASK_NAME, TimeSpan.FromSeconds(60));
                System.Diagnostics.Debug.WriteLine("Periodic task is started: " + TASK_NAME);
#endif
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("BNS Error: The action is disabled") && !HaruSettings.BackgroundAgentDisabledShown)
                {
                    HaruSettings.BackgroundAgentDisabledShown = true;
                    HaruSettings.Save();
                    MessageBox.Show(AppResources.BackgroundAgentDisabled);
                }
            }
            catch (SchedulerServiceException) { }
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