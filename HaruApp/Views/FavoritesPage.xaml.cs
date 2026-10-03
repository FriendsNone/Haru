using HaruApp.Helpers;
using HaruApp.Resources;
using HaruApp.ViewModels;
using HaruCore;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace HaruApp.Views
{
    public partial class FavoritesPage : PhoneApplicationPage
    {
        private readonly OpenMeteoClient client = new OpenMeteoClient();
        private readonly ProgressIndicator progressIndicator = new ProgressIndicator();
        private readonly FavoritesViewModel vm = new FavoritesViewModel();
        private readonly DispatcherTimer timer;
        private int pending;

        public FavoritesPage()
        {
            InitializeComponent();
            DataContext = vm;
            DescriptionTextBlock.Text = string.Format(AppResources.FavoritesDescription, HaruSettings.MaxFavorites);
            SystemTray.SetProgressIndicator(this, progressIndicator);
            timer = ProgressHelper.CreateProgressTimer(progressIndicator);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            LoadFavorites();
            FetchForecasts();
        }

        private void FetchForecasts()
        {
            var favorites = vm.Favorites.Select(r => r.Place).ToList();
            if (pending > 0 || favorites.Count == 0) return;

            var temperatureUnit = HaruSettings.TemperatureUnit;
            var windSpeedUnit = HaruSettings.WindSpeedUnit;
            var precipitationUnit = HaruSettings.PrecipitationUnit;
            var failed = false;

            pending = favorites.Count;
            ProgressHelper.ShowProgress(progressIndicator, AppResources.ProgressFetchingForecast, timer: timer);

            foreach (var place in favorites)
            {
                client.GetForecast(place.Latitude, place.Longitude, temperatureUnit, windSpeedUnit, precipitationUnit, (forecast, error) =>
                {
                    if (error != null) failed = true;

                    if (forecast != null && error == null)
                    {
                        var current = forecast.ToCurrentRecord();
                        FavoritesHelper.UpdateTiles(place, current);

                        var record = vm.Favorites.FirstOrDefault(r => r.Place.IsSameAs(place));
                        if (record != null) record.Current = current;
                    }

                    if (--pending > 0) return;

                    if (failed)
                        ProgressHelper.ShowProgress(progressIndicator, AppResources.ProgressShowingLastUpdate, true, timer);
                    else
                        ProgressHelper.HideProgress(progressIndicator, timer);
                });
            }
        }

        private void LoadFavorites()
        {
            var favorites = HaruSettings.Favorites;
            vm.Favorites = favorites
                .Select((f, i) => new FavoriteRecord
                {
                    Place = f,
                    IsHome = i == 0,
                    IsPinned = TileHelper.FindTile(f) != null,
                    CanRemove = i > 0 || favorites.Count > 1,
                    Current = FavoritesHelper.GetCachedCurrent(f)
                })
                .ToList();
        }

        private void FavoritesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = FavoritesListBox.SelectedItem as FavoriteRecord;
            if (selected == null) return;

            NavigationService.Navigate(new Uri(selected.IsHome
                ? "/Views/MainPage.xaml?refresh=true"
                : TileHelper.NavigationUriFor(selected.Place).OriginalString + "&refresh=true", UriKind.Relative));
        }

        private void SetHomeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var record = ((FrameworkElement)sender).DataContext as FavoriteRecord;
            if (record == null) return;

            FavoritesHelper.SetHome(record.Place);
            LoadFavorites();
        }

        private void PinMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var record = ((FrameworkElement)sender).DataContext as FavoriteRecord;
            if (record != null)
                FavoritesHelper.Pin(record.Place, null);
        }

        private void RemoveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var record = ((FrameworkElement)sender).DataContext as FavoriteRecord;
            if (record != null)
                FavoritesHelper.Remove(record.Place, LoadFavorites);
        }
    }
}
