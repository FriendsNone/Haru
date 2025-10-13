using System;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using HaruCore;
using HaruApp.ViewModels;
using HaruApp.Helpers;

namespace HaruApp.Views
{
    public partial class SearchPage : PhoneApplicationPage
    {
        private readonly IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
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

            settings["Location"] = selectedLocation.NameShort;
            settings["Latitude"] = selectedLocation.Latitude;
            settings["Longitude"] = selectedLocation.Longitude;
            settings.Save();

            NavigationService.GoBack();
        }

        private void FetchLocation(string searchTerm)
        {
            ProgressHelper.ShowProgress(progressIndicator, string.Format("Searching for \"{0}\"", searchTerm));

            client.SearchLocation(searchTerm, (locations, error) =>
            {
                if (error != null)
                {
                    ProgressHelper.ShowProgress(progressIndicator, "Something went wrong. Try again later.", true, timer);
                    return;
                }

                if (locations == null || locations.Location == null)
                {
                    ProgressHelper.ShowProgress(progressIndicator, string.Format("No results for \"{0}\"", searchTerm), true, timer);
                    return;
                }

                vm.Location = locations.ToLocationRecords();
                ProgressHelper.HideProgress(progressIndicator);
            });
        }
    }
}