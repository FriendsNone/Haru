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

        //private void PinApplicationBarIconButton_Click(object sender, EventArgs e)
        //{
        //    // TODO: Implement pin to start functionality
        //}

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
                    WeatherCodeImage.Source           = new BitmapImage(new Uri(WeatherInterpretationModel.GetWeatherIcon(cw.WeatherCode, cw.IsDay), UriKind.Relative));
                    TemperatureTextBlock.Text         = string.Format("{0}{1}", cw.Temperature, UnitModel.GetTemperatureUnit(temperatureUnit));
                    ApparentTemperatureTextBlock.Text = string.Format("feels like {0}{1}", cw.ApparentTemperature, UnitModel.GetTemperatureUnit(temperatureUnit));
                    WeatherCodeTextBlock.Text         = WeatherInterpretationModel.GetWeatherDescription(cw.WeatherCode, cw.IsDay);
                    RelativeHumidityTextBlock.Text    = string.Format("{0}%", cw.RelativeHumidity);
                    PrecipitationTextBlock.Text       = string.Format("{0} {1}", cw.Precipitation, UnitModel.GetPrecipitationUnit(precipitationUnit));
                    //RainTextBlock.Text                = string.Format("{0}{1}", cw.Rain, UnitModel.GetPrecipitationUnit(precipitationUnit));
                    //ShowersTextBlock.Text             = string.Format("{0}{1}", cw.Showers, UnitModel.GetPrecipitationUnit(precipitationUnit));
                    //SnowfallTextBlock.Text            = string.Format("{0}{1}", cw.Snowfall, UnitModel.GetPrecipitationUnit(precipitationUnit));
                    CloudCoverTextBlock.Text          = string.Format("{0}%", cw.CloudCover);
                    PressureTextBlock.Text            = string.Format("{0} hPa", cw.Pressure);
                    SurfacePressureTextBlock.Text     = string.Format("{0} hPa", cw.SurfacePressure);
                    WindSpeedTextBlock.Text           = string.Format("{0} {1}", cw.WindSpeed, UnitModel.GetWindSpeedUnit(windSpeedUnit));
                    WindDirectionTextBlock.Text       = string.Format("{0}°", cw.WindDirection);
                    WindGustsTextBlock.Text           = string.Format("{0} {1}", cw.WindGusts, UnitModel.GetWindSpeedUnit(windSpeedUnit));
                    NowScrollViewer.Visibility        = Visibility.Visible;

                    UpdateTile(cw);
                    progressIndicator.IsVisible = false;
                }
            });
        }

        private void UpdateTile(CurrentWeather cw)
        {
            ShellTile tile = ShellTile.ActiveTiles.First();
            if (tile != null)
            {
                string location = (string)settings["Location"];

                StandardTileData data = new StandardTileData()
                {
                    Title = location,
                    BackgroundImage = new Uri(WeatherInterpretationModel.GetWeatherIcon(cw.WeatherCode, cw.IsDay), UriKind.Relative),
                    BackTitle = string.Format("{0}", DateTime.Now.ToString("t")),
                    BackContent = string.Format("{0}°{1}\n{2}",
                        cw.Temperature,
                        (string)settings["TemperatureUnit"] == "celsius" ? "C" : "F",
                        WeatherInterpretationModel.GetWeatherDescription(cw.WeatherCode, cw.IsDay)),
                };


                tile.Update(data);
            }
        }
    }
}