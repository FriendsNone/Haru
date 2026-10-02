using HaruApp.Helpers;
using HaruApp.Resources;
using HaruApp.ViewModels;
using HaruCore;
using Microsoft.Phone.Controls;
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
        private static readonly TimeSpan RefetchAfter = TimeSpan.FromMinutes(30);
        private static bool agentStarted;

        private readonly OpenMeteoClient client = new OpenMeteoClient();
        private readonly ProgressIndicator progressIndicator = new ProgressIndicator();
        private readonly ForecastViewModel vm = new ForecastViewModel();
        private readonly DispatcherTimer timer;
        private readonly DispatcherTimer forecastTimeTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
        private ApplicationBarIconButton favoriteButton;
        private ApplicationBarIconButton pinButton;
        private Place place;
        private bool isFetching;

        public MainPage()
        {
            InitializeComponent();
            DataContext = vm;
            timer = ProgressHelper.CreateProgressTimer(progressIndicator);
            forecastTimeTimer.Tick += (s, e) => vm.RefreshForecastTime();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            SystemTray.ProgressIndicator = progressIndicator;

            if (!agentStarted)
            {
                agentStarted = true;
                AgentHelper.StartPeriodicAgent();
            }

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

            if (NavigationContext.QueryString.ContainsKey("refresh"))
            {
                while (NavigationService.CanGoBack)
                    NavigationService.RemoveBackEntry();
            }

            if (ApplicationBar == null)
                BuildApplicationBar(NavigationContext.QueryString.ContainsKey("search"));

            ShowPlace();
            vm.RefreshForecastTime();
            forecastTimeTimer.Start();
        }

        private void ShowPlace()
        {
            var next = TileHelper.ParsePlace(NavigationContext.QueryString) ?? HaruSettings.HomePlace;

            if (next != null && (!next.IsSameAs(place) || next.Name != place.Name))
            {
                place = next;
                MainPivot.Title = place.Name.ToUpper();
                if (MainPivot.SelectedIndex != 0) MainPivot.SelectedIndex = 0;
                FetchForecast();
            }
            else if (!isFetching && vm.Current != null && DateTime.UtcNow - vm.Current.ObservedUtc > RefetchAfter)
            {
                FetchForecast();
            }

            UpdateApplicationBarButtons();
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

        private void UpdateApplicationBarButtons()
        {
            if (favoriteButton == null) return;

            var isFavorite = place != null && FavoritesHelper.IsFavorite(place);
            var isHome = place != null && FavoritesHelper.IsHome(place);

            favoriteButton.IconUri = new Uri(isFavorite
                ? "/Assets/AppBar/appbar.favs.removefrom.rest.png"
                : "/Assets/AppBar/appbar.favs.addto.rest.png", UriKind.Relative);
            favoriteButton.Text = isFavorite ? AppResources.AppBarRemoveFromFavorites : AppResources.AppBarAddToFavorites;
            favoriteButton.IsEnabled = place != null && (!isFavorite || FavoritesHelper.CanRemove(place));
            pinButton.IsEnabled = place != null && (isHome || TileHelper.FindTile(place) == null);
        }

        private void ToggleFavoriteApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            if (place == null) return;

            if (FavoritesHelper.IsFavorite(place))
                FavoritesHelper.Remove(place, ShowPlace);
            else if (FavoritesHelper.TryAdd(place))
                UpdateApplicationBarButtons();
        }

        private void PinApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            if (place == null) return;

            FavoritesHelper.Pin(place, vm.Current);
            UpdateApplicationBarButtons();
        }

        private void FavoritesApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/FavoritesPage.xaml", UriKind.Relative));
        }

        private void SearchApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/SearchPage.xaml", UriKind.Relative));
        }

        private void RefreshApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            if (place == null)
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
            var target = place;
            var temperatureUnit = HaruSettings.TemperatureUnit;
            var windSpeedUnit = HaruSettings.WindSpeedUnit;
            var precipitationUnit = HaruSettings.PrecipitationUnit;

            isFetching = true;
            ProgressHelper.ShowProgress(progressIndicator, AppResources.ProgressFetchingForecast, timer: timer);

            client.GetForecast(target.Latitude, target.Longitude, temperatureUnit, windSpeedUnit, precipitationUnit, (forecast, error) =>
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
                UpdateTiles(target, vm.Current);

                if (error != null)
                    ProgressHelper.ShowProgress(progressIndicator, AppResources.ProgressShowingLastUpdate, true, timer);
                else
                    ProgressHelper.HideProgress(progressIndicator, timer);
            });
        }

        private static void UpdateTiles(Place target, CurrentRecord cr)
        {
            var live = HaruSettings.BackgroundUpdateEnabled && HaruSettings.LiveTileEnabled;
            var mono = HaruSettings.MonochromeTileEnabled;

            var home = HaruSettings.HomePlace;
            if (target.IsSameAs(home))
            {
                if (live)
                    TileHelper.UpdateTile(TileHelper.PrimaryTile, home.Name, cr, mono);
                else
                    TileHelper.ResetTile();
            }

            var tile = TileHelper.FindTile(target);
            if (tile != null)
                TileHelper.UpdateTile(tile, target.Name, live ? cr : null, mono);
        }

        private void BuildApplicationBar(bool isSearchResult)
        {
            ApplicationBar = new ApplicationBar();

            if (isSearchResult)
            {
                favoriteButton = new ApplicationBarIconButton();
                favoriteButton.IconUri = new Uri("/Assets/AppBar/appbar.favs.addto.rest.png", UriKind.Relative);
                favoriteButton.Text = AppResources.AppBarAddToFavorites;
                favoriteButton.Click += ToggleFavoriteApplicationBarIconButton_Click;
                ApplicationBar.Buttons.Add(favoriteButton);

                pinButton = new ApplicationBarIconButton();
                pinButton.IconUri = new Uri("/Assets/AppBar/appbar.pin.rest.png", UriKind.Relative);
                pinButton.Text = AppResources.AppBarPinToStart;
                pinButton.Click += PinApplicationBarIconButton_Click;
                ApplicationBar.Buttons.Add(pinButton);
            }
            else
            {
                ApplicationBarIconButton favoritesButton = new ApplicationBarIconButton();
                favoritesButton.IconUri = new Uri("/Assets/AppBar/appbar.favs.rest.png", UriKind.Relative);
                favoritesButton.Text = AppResources.AppBarFavorites;
                favoritesButton.Click += FavoritesApplicationBarIconButton_Click;
                ApplicationBar.Buttons.Add(favoritesButton);

                ApplicationBarIconButton searchButton = new ApplicationBarIconButton();
                searchButton.IconUri = new Uri("/Assets/AppBar/appbar.feature.search.rest.png", UriKind.Relative);
                searchButton.Text = AppResources.AppBarSearch;
                searchButton.Click += SearchApplicationBarIconButton_Click;
                ApplicationBar.Buttons.Add(searchButton);
            }

            ApplicationBarIconButton refreshButton = new ApplicationBarIconButton();
            refreshButton.IconUri = new Uri("/Assets/AppBar/appbar.refresh.rest.png", UriKind.Relative);
            refreshButton.Text = AppResources.AppBarRefresh;
            refreshButton.Click += RefreshApplicationBarIconButton_Click;
            ApplicationBar.Buttons.Add(refreshButton);

            if (isSearchResult)
                return;

            ApplicationBarMenuItem settingsItem = new ApplicationBarMenuItem(AppResources.AppBarSettings);
            settingsItem.Click += SettingsApplicationBarMenuItem_Click;
            ApplicationBar.MenuItems.Add(settingsItem);

            ApplicationBarMenuItem aboutItem = new ApplicationBarMenuItem(AppResources.AppBarAbout);
            aboutItem.Click += AboutApplicationBarMenuItem_Click;
            ApplicationBar.MenuItems.Add(aboutItem);
        }
    }
}