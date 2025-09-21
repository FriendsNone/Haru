using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using HaruCore;
using System.Windows.Input;
using System.Windows.Threading;

namespace HaruApp.Views
{
    public partial class SearchPage : PhoneApplicationPage
    {
        private IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        private ProgressIndicator progressIndicator;
        private OpenMeteoClient client;
        private DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };

        public SearchPage()
        {
            InitializeComponent();
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
            SearchPhoneTextBox.Focus();
        }

        private void SearchPhoneTextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string searchTerm = SearchPhoneTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return;

                FetchLocation(searchTerm);
                this.Focus();
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = SearchPhoneTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(searchTerm))
                return;

            FetchLocation(searchTerm);
        }

        private void ResultListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedLocation = ResultListBox.SelectedItem as LocationResult;
            if (selectedLocation == null)
                return;

            settings["Location"] = string.Format("{0}, {1}", selectedLocation.Name, selectedLocation.CountryCode);
            settings["Latitude"] = selectedLocation.Latitude;
            settings["Longitude"] = selectedLocation.Longitude;
            settings.Save();

            NavigationService.GoBack();
        }

        private void FetchLocation(string searchTerm)
        {
            progressIndicator.IsIndeterminate = true;
            progressIndicator.Text = string.Format("Searching for \"{0}\"", searchTerm);
            progressIndicator.IsVisible = true;

            client.SearchLocation(searchTerm, (locations, err) =>
            {
                if (err != null)
                {
                    progressIndicator.IsIndeterminate = false;
                    progressIndicator.Text = "Something went wrong. Try again later.";
                    timer.Start();
                    return;
                }

                if (locations == null || locations.Count == 0)
                {
                    progressIndicator.IsIndeterminate = false;
                    progressIndicator.Text = string.Format("No results for \"{0}\"", searchTerm);
                    timer.Start();
                    return;
                }

                ResultListBox.ItemsSource = locations;
                progressIndicator.IsVisible = false;
            });
        }
    }
}