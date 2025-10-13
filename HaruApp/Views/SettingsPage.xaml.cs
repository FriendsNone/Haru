using System;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using HaruApp.Helpers;

namespace HaruApp.Views
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        private readonly IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        private bool isPromptShown;

        public SettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (settings.Contains("BackgroundUpdateEnable"))
                BackgroundUpdateToggleSwitch.IsChecked = (bool)settings["BackgroundUpdateEnable"];
            if (settings.Contains("TemperatureUnit"))
                TemperatureUnitListPicker.SelectedItem = settings["TemperatureUnit"];
            if (settings.Contains("WindSpeedUnit"))
                WindSpeedUnitListPicker.SelectedItem = settings["WindSpeedUnit"];
            if (settings.Contains("PrecipitationUnit"))
                PrecipitationUnitListPicker.SelectedItem = settings["PrecipitationUnit"];
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (isPromptShown || !HasChanges()) return;

            e.Cancel = true;
            isPromptShown = true;

            Dispatcher.BeginInvoke(() =>
            {
                PromptHelper.ShowPrompt(
                    "Do you want to save changes?",
                    "Your changes will be lost if you don't save them.",
                    "save",
                    "don't save",
                    () =>
                    {
                        SaveSettings();
                        NavigationService.Navigate(new Uri("/Views/MainPage.xaml?refresh=true", UriKind.Relative));
                    },
                    () => NavigationService.GoBack(),
                    () => isPromptShown = false);
            });
        }

        private void ClearSavedForecastButton_Click(object sender, RoutedEventArgs e)
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists("forecast.json"))
                    store.DeleteFile("forecast.json");
            }

            PromptHelper.ShowAlert(
                "Saved forecast cleared!",
                "A fresh and up-to-date forecast will be ready the next time you refresh or open the app.");
        }

        private void SaveApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            SaveSettings();
            NavigationService.Navigate(new Uri("/Views/MainPage.xaml?refresh=true", UriKind.Relative));
        }

        private void CancelApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }

        private bool HasChanges()
        {
            return (settings.Contains("BackgroundUpdateEnable") && BackgroundUpdateToggleSwitch.IsChecked != (bool?)settings["BackgroundUpdateEnable"]) ||
                   (settings.Contains("TemperatureUnit") && TemperatureUnitListPicker.SelectedItem as string != settings["TemperatureUnit"] as string) ||
                   (settings.Contains("WindSpeedUnit") && WindSpeedUnitListPicker.SelectedItem as string != settings["WindSpeedUnit"] as string) ||
                   (settings.Contains("PrecipitationUnit") && PrecipitationUnitListPicker.SelectedItem as string != settings["PrecipitationUnit"] as string);
        }

        private void SaveSettings()
        {
            settings["BackgroundUpdateEnable"] = BackgroundUpdateToggleSwitch.IsChecked;
            settings["TemperatureUnit"] = TemperatureUnitListPicker.SelectedItem as string;
            settings["WindSpeedUnit"] = WindSpeedUnitListPicker.SelectedItem as string;
            settings["PrecipitationUnit"] = PrecipitationUnitListPicker.SelectedItem as string;
            settings.Save();
        }
    }
}