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
using System.Windows.Media.Imaging;
using HaruApp.ViewModels;

namespace HaruApp.Views
{
    public partial class MainPage : PhoneApplicationPage
    {
        private IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        private ProgressIndicator progressIndicator;
        private OpenMeteoClient client;
        private string lastLocation;
        private ForecastViewModel vm = new ForecastViewModel();

        public MainPage()
        {
            InitializeComponent();
            this.DataContext = vm;
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
            NavigationService.Navigate(new Uri("/Views/SearchPage.xaml", UriKind.Relative));
        }

        private void RefreshApplicationBarIconButton_Click(object sender, EventArgs e)
        {
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
            double latitude = (double)settings["Latitude"];
            double longitude = (double)settings["Longitude"];

            string temperatureUnit = (string)settings["TemperatureUnit"];
            string windSpeedUnit = (string)settings["WindSpeedUnit"];
            string precipitationUnit = (string)settings["PrecipitationUnit"];

            progressIndicator.Text = "Fetching forecast...";
            progressIndicator.IsVisible = true;

            client.GetForecast(latitude, longitude, temperatureUnit, windSpeedUnit, precipitationUnit, (forecast, ferr) =>
            {
                if (ferr != null || forecast == null)
                    return;

                vm.Current = forecast.ToCurrentRecord();
                vm.Hourly = forecast.ToHourlyRecords();
                vm.Daily = forecast.ToDailyRecords();

                NowScrollViewer.Visibility = Visibility.Visible;
                UpdateTile(forecast.Current);
                progressIndicator.IsVisible = false;
            });
        }

        private void UpdateTile(CurrentWeather cw)
        {
            ShellTile tile = ShellTile.ActiveTiles.First();
            if (tile != null)
            {
                string location = (string)settings["Location"];
                string temperatureUnit = (string)settings["TemperatureUnit"];

                StandardTileData data = new StandardTileData()
                {
                    Title = location,
                    BackgroundImage = new Uri(WeatherInterpretationModel.GetWeatherTileIcon(cw.WeatherCode, cw.IsDay), UriKind.Relative),
                    BackTitle = DateTime.Now.ToString("t"),
                    BackContent = string.Format("{0}{1}\n{2}",
                        System.Math.Ceiling(cw.Temperature),
                        UnitModel.GetTemperatureUnit(temperatureUnit),
                        WeatherInterpretationModel.GetWeatherDescription(cw.WeatherCode, cw.IsDay)),
                };

                tile.Update(data);
            }
        }
    }
}