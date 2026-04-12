using HaruApp.Helpers;
using HaruApp.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Navigation;

namespace HaruApp.Views
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        private readonly IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        private bool isPromptShown;

        public SettingsPage()
        {
            InitializeComponent();
            BuildApplicationBar();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (settings.Contains("BackgroundUpdateEnable"))
                BackgroundUpdateToggleSwitch.IsChecked = (bool)settings["BackgroundUpdateEnable"];
            if (settings.Contains("MonochromeTileEnable"))
                MonochromeTileToggleSwitch.IsChecked = (bool)settings["MonochromeTileEnable"];
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
                    AppResources.PromptSaveChangesTitle,
                    AppResources.PromptSaveChangesMessage,
                    AppResources.PromptSave,
                    AppResources.PromptDontSave,
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
                AppResources.SettingClearForecastSuccessTitle,
                AppResources.SettingClearForecastSuccessMessage,
                AppResources.PromptOkay);
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
                   (settings.Contains("MonochromeTileEnable") && MonochromeTileToggleSwitch.IsChecked != (bool?)settings["MonochromeTileEnable"]) ||
                   (settings.Contains("TemperatureUnit") && TemperatureUnitListPicker.SelectedItem as string != settings["TemperatureUnit"] as string) ||
                   (settings.Contains("WindSpeedUnit") && WindSpeedUnitListPicker.SelectedItem as string != settings["WindSpeedUnit"] as string) ||
                   (settings.Contains("PrecipitationUnit") && PrecipitationUnitListPicker.SelectedItem as string != settings["PrecipitationUnit"] as string);
        }

        private void SaveSettings()
        {
            settings["BackgroundUpdateEnable"] = BackgroundUpdateToggleSwitch.IsChecked;
            settings["MonochromeTileEnable"] = MonochromeTileToggleSwitch.IsChecked;
            settings["TemperatureUnit"] = TemperatureUnitListPicker.SelectedItem as string;
            settings["WindSpeedUnit"] = WindSpeedUnitListPicker.SelectedItem as string;
            settings["PrecipitationUnit"] = PrecipitationUnitListPicker.SelectedItem as string;
            settings.Save();
        }

        private void BuildApplicationBar()
        {
            ApplicationBar = new ApplicationBar();

            ApplicationBarIconButton saveButton = new ApplicationBarIconButton();
            saveButton.IconUri = new Uri("/Assets/AppBar/appbar.save.rest.png", UriKind.Relative);
            saveButton.Text = AppResources.AppBarSave;
            saveButton.Click += SaveApplicationBarIconButton_Click;
            ApplicationBar.Buttons.Add(saveButton);

            ApplicationBarIconButton cancelButton = new ApplicationBarIconButton();
            cancelButton.IconUri = new Uri("/Assets/AppBar/appbar.cancel.rest.png", UriKind.Relative);
            cancelButton.Text = AppResources.AppBarCancel;
            cancelButton.Click += CancelApplicationBarIconButton_Click;
            ApplicationBar.Buttons.Add(cancelButton);
        }
    }
}