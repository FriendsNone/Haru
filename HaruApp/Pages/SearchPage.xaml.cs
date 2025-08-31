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

namespace HaruApp.Pages
{
    public partial class SearchPage : PhoneApplicationPage
    {
        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        private ProgressIndicator progressIndicator;
        private OpenMeteoClient client;

        public SearchPage()
        {
            InitializeComponent();
            client = new OpenMeteoClient();
            progressIndicator = new ProgressIndicator()
            {
                IsIndeterminate = true
            };
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            SystemTray.ProgressIndicator = progressIndicator;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = SearchPhoneTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return;
            }

            progressIndicator.IsVisible = true;
            progressIndicator.Text = string.Format("Searching for \"{0}\"", searchTerm);

            client.SearchLocation(searchTerm, (locations, err) =>
            {
                if (err != null)
                {
                    progressIndicator.IsVisible = false;
                    return;
                }
                if (locations == null || locations.Count == 0)
                {
                    progressIndicator.IsVisible = false;
                    return;
                }

                ResultListBox.ItemsSource = locations;
                progressIndicator.IsVisible = false;
            });
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
    }
}