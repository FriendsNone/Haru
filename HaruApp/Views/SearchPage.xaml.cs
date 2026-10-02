using HaruApp.Helpers;
using HaruApp.Resources;
using HaruApp.ViewModels;
using HaruCore;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Device.Location;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace HaruApp.Views
{
    public partial class SearchPage : PhoneApplicationPage
    {
        private readonly ProgressIndicator progressIndicator = new ProgressIndicator();
        private readonly OpenMeteoClient client = new OpenMeteoClient();
        private readonly GeocodingViewModel vm = new GeocodingViewModel();
        private readonly DispatcherTimer timer;
        private readonly DispatcherTimer locateTimeout = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        private GeoCoordinateWatcher watcher;

        public SearchPage()
        {
            InitializeComponent();
            DataContext = vm;
            timer = ProgressHelper.CreateProgressTimer(progressIndicator);
            locateTimeout.Tick += (s, e) => LocationFailed();
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            StopWatcher();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            SystemTray.ProgressIndicator = progressIndicator;
            if (vm.Location == null)
                SearchPhoneTextBox.Focus();
        }

        private void SearchPhoneTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var searchTerm = SearchPhoneTextBox.Text.Trim();
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    FetchLocation(searchTerm);
                    Focus();
                }
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            var searchTerm = SearchPhoneTextBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(searchTerm))
                FetchLocation(searchTerm);
        }

        private void ResultListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedLocation = ResultListBox.SelectedItem as LocationRecord;
            if (selectedLocation == null) return;

            ResultListBox.SelectedItem = null;
            ShowLocation(ToPlace(selectedLocation));
        }

        private void ResultContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            var menu = (ContextMenu)sender;
            var place = ToPlace(menu.DataContext as LocationRecord);
            if (place == null) return;

            var isFavorite = FavoritesHelper.IsFavorite(place);
            var favoriteItem = (MenuItem)menu.Items[0];
            favoriteItem.Header = isFavorite ? AppResources.MenuRemoveFavorite : AppResources.MenuAddFavorite;
            favoriteItem.IsEnabled = !isFavorite || FavoritesHelper.CanRemove(place);

            var pinItem = (MenuItem)menu.Items[1];
            pinItem.IsEnabled = FavoritesHelper.IsHome(place) || TileHelper.FindTile(place) == null;
        }

        private void FavoriteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var place = ToPlace(((FrameworkElement)sender).DataContext as LocationRecord);
            if (place == null) return;

            if (FavoritesHelper.IsFavorite(place))
                FavoritesHelper.Remove(place, null);
            else
                FavoritesHelper.TryAdd(place);
        }

        private void PinMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var place = ToPlace(((FrameworkElement)sender).DataContext as LocationRecord);
            if (place != null)
                FavoritesHelper.Pin(place, null);
        }

        private static Place ToPlace(LocationRecord record)
        {
            return record == null
                ? null
                : new Place { Name = record.NameShort, Latitude = record.Latitude, Longitude = record.Longitude };
        }

        private void CurrentLocationButton_Click(object sender, RoutedEventArgs e)
        {
            if (watcher != null) return;

            if (HaruSettings.LocationServiceEnabled)
            {
                StartLocating();
                return;
            }

            PromptHelper.ShowPrompt(
                AppResources.PromptLocationTitle,
                AppResources.PromptLocationMessage,
                AppResources.PromptAllow,
                AppResources.PromptDontAllow,
                () =>
                {
                    HaruSettings.LocationServiceEnabled = true;
                    HaruSettings.Save();
                    StartLocating();
                });
        }

        private void StartLocating()
        {
            ProgressHelper.ShowProgress(progressIndicator, AppResources.ProgressLocating, timer: timer);
            watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default);
            watcher.StatusChanged += Watcher_StatusChanged;
            watcher.PositionChanged += Watcher_PositionChanged;
            watcher.Start();
            locateTimeout.Start();
        }

        private void Watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            if (e.Status == GeoPositionStatus.Disabled)
                Dispatcher.BeginInvoke(LocationFailed);
        }

        private void Watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            var coordinate = e.Position.Location;
            if (coordinate.IsUnknown) return;

            Dispatcher.BeginInvoke(() =>
            {
                if (watcher == null) return;
                StopWatcher();

                client.ReverseGeocode(coordinate.Latitude, coordinate.Longitude, (place, error) =>
                    Dispatcher.BeginInvoke(() =>
                    {
                        ProgressHelper.HideProgress(progressIndicator, timer);

                        // Without a place name, the coordinates double as the name.
                        var name = place ?? string.Format(CultureInfo.InvariantCulture, "{0:0.##}, {1:0.##}",
                            coordinate.Latitude, coordinate.Longitude);
                        ShowLocation(new Place { Name = name, Latitude = coordinate.Latitude, Longitude = coordinate.Longitude });
                    }));
            });
        }

        private void LocationFailed()
        {
            if (watcher == null) return;
            StopWatcher();
            ProgressHelper.ShowProgress(progressIndicator, AppResources.ProgressLocationUnavailable, true, timer);
        }

        private void StopWatcher()
        {
            locateTimeout.Stop();
            if (watcher == null) return;

            watcher.StatusChanged -= Watcher_StatusChanged;
            watcher.PositionChanged -= Watcher_PositionChanged;
            watcher.Stop();
            watcher.Dispose();
            watcher = null;
        }

        private void ShowLocation(Place place)
        {
            if (!HaruSettings.HasLocation)
            {
                FavoritesHelper.TryAdd(place);
                NavigationService.Navigate(new Uri("/Views/MainPage.xaml?refresh=true", UriKind.Relative));
                return;
            }

            NavigationService.Navigate(new Uri(TileHelper.NavigationUriFor(place).OriginalString + "&search=true", UriKind.Relative));
        }

        private void FetchLocation(string searchTerm)
        {
            ProgressHelper.ShowProgress(progressIndicator, string.Format(AppResources.ProgressSearching, searchTerm), timer: timer);

            client.SearchLocation(searchTerm, (locations, error) =>
            {
                if (error != null)
                {
                    ProgressHelper.ShowProgress(progressIndicator, AppResources.ProgressError, true, timer);
                    return;
                }

                if (locations == null || locations.Location == null)
                {
                    ProgressHelper.ShowProgress(progressIndicator, string.Format(AppResources.ProgressNoResults, searchTerm), true, timer);
                    return;
                }

                vm.Location = locations.ToLocationRecords();
                ProgressHelper.HideProgress(progressIndicator, timer);
            });
        }
    }
}