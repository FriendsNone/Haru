using HaruApp.Helpers;
using HaruApp.Resources;
using HaruApp.ViewModels;
using HaruCore;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
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

        public SearchPage()
        {
            InitializeComponent();
            DataContext = vm;
            timer = ProgressHelper.CreateProgressTimer(progressIndicator);
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            SystemTray.ProgressIndicator = progressIndicator;
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

            HaruSettings.Location = selectedLocation.NameShort;
            HaruSettings.Latitude = selectedLocation.Latitude;
            HaruSettings.Longitude = selectedLocation.Longitude;
            HaruSettings.Save();

            if (NavigationService.CanGoBack)
                NavigationService.RemoveBackEntry();

            NavigationService.Navigate(new Uri("/Views/MainPage.xaml?refresh=true", UriKind.Relative));
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