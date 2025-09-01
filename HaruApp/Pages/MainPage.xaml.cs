using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using System.Windows.Navigation;
using HaruCore;

namespace HaruApp
{
    public partial class MainPage : PhoneApplicationPage
    {
        private IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        private ProgressIndicator progressIndicator;
        private OpenMeteoClient client;
        private string lastLocation;

        public MainPage()
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

            if (!settings.Contains("Location"))
                GuideCanvas.Visibility = Visibility.Visible;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (settings.Contains("Location") && settings["Location"] as string != lastLocation)
            {
                string location = settings["Location"] as string;
                lastLocation = location;

                GuideCanvas.Visibility = Visibility.Collapsed;
                MainPivot.Title = location.ToUpper();

                FetchForecast();
            }

        }

        private void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainPivot.SelectedIndex == 0)
                if (ApplicationBar != null)
                    ApplicationBar.Mode = ApplicationBarMode.Default;
                else
                if (ApplicationBar != null)
                    ApplicationBar.Mode = ApplicationBarMode.Minimized;
        }

        private void SearchApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/SearchPage.xaml", UriKind.Relative));
        }

        private void RefreshApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            FetchForecast();
        }

        private void PinApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            // TODO: Implement pin to start functionality
        }

        private void SettingsApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/SettingsPage.xaml", UriKind.Relative));
        }

        private void AboutApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/AboutPage.xaml", UriKind.Relative));
        }

        private void FetchForecast()
        {
            double latitude = (double)settings["Latitude"];
            double longitude = (double)settings["Longitude"];

            string temperatureUnit = (string)settings["TemperatureUnit"];
            string windSpeedUnit = (string)settings["WindSpeedUnit"];
            string precipitationUnit = (string)settings["PrecipitationUnit"];

            progressIndicator.Text = "Fetching forecast...";
            progressIndicator.IsVisible = true;

            NowTextBlock.Text = "";

            client.GetForecast(latitude, longitude, temperatureUnit, windSpeedUnit, precipitationUnit, (forecast, ferr) =>
            {
                if (ferr != null || forecast == null)
                {
                    progressIndicator.IsVisible = false;
                    return;
                }

                CurrentWeather cw = forecast.CurrentWeather;
                if (cw != null)
                {
                    NowTextBlock.Text = string.Format("Current temperature: {0}°C\nWindspeed: {1} km/h\nWind direction: {2}°\n{3}",
                        cw.Temperature,
                        cw.WindSpeed,
                        cw.WindDirection,
                        cw.Time);
                    progressIndicator.IsVisible = false;
                }
            });
        }
    }
}