using HaruApp.Helpers;
using HaruApp.Resources;
using HaruApp.ViewModels;
using HaruCore;
using Microsoft.Phone.Controls;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace HaruApp.Views
{
    public partial class FavoritesPage : PhoneApplicationPage
    {
        private readonly FavoritesViewModel vm = new FavoritesViewModel();

        public FavoritesPage()
        {
            InitializeComponent();
            DataContext = vm;
            DescriptionTextBlock.Text = string.Format(AppResources.FavoritesDescription, HaruSettings.MaxFavorites);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            LoadFavorites();
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
                    CanRemove = i > 0 || favorites.Count > 1
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
