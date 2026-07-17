using HaruApp.Helpers;
using HaruApp.Resources;
using HaruCore;
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
        private bool suppressToggleEvents;

        public SettingsPage()
        {
            InitializeComponent();
            BuildApplicationBar();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            suppressToggleEvents = true;
            BackgroundUpdateToggleSwitch.IsChecked = SettingsHelper.GetBool(settings, "BackgroundUpdateEnable", true);
            LiveTileToggleSwitch.IsChecked = SettingsHelper.GetBool(settings, "LiveTileEnable", true);
            NotificationToggleSwitch.IsChecked = SettingsHelper.GetBool(settings, "NotificationEnable", true);
            MonochromeTileToggleSwitch.IsChecked = SettingsHelper.GetBool(settings, "MonochromeTileEnable", false);
            suppressToggleEvents = false;
            ApplyToggleDependencies();

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
            return BackgroundUpdateToggleSwitch.IsChecked != SettingsHelper.GetBool(settings, "BackgroundUpdateEnable", true) ||
                   LiveTileToggleSwitch.IsChecked != SettingsHelper.GetBool(settings, "LiveTileEnable", true) ||
                   NotificationToggleSwitch.IsChecked != SettingsHelper.GetBool(settings, "NotificationEnable", true) ||
                   MonochromeTileToggleSwitch.IsChecked != SettingsHelper.GetBool(settings, "MonochromeTileEnable", false) ||
                   (settings.Contains("TemperatureUnit") && TemperatureUnitListPicker.SelectedItem as string != settings["TemperatureUnit"] as string) ||
                   (settings.Contains("WindSpeedUnit") && WindSpeedUnitListPicker.SelectedItem as string != settings["WindSpeedUnit"] as string) ||
                   (settings.Contains("PrecipitationUnit") && PrecipitationUnitListPicker.SelectedItem as string != settings["PrecipitationUnit"] as string);
        }

        private void SaveSettings()
        {
            settings["BackgroundUpdateEnable"] = BackgroundUpdateToggleSwitch.IsChecked;
            settings["LiveTileEnable"] = LiveTileToggleSwitch.IsChecked;
            settings["NotificationEnable"] = NotificationToggleSwitch.IsChecked;
            settings["MonochromeTileEnable"] = MonochromeTileToggleSwitch.IsChecked;
            settings["TemperatureUnit"] = TemperatureUnitListPicker.SelectedItem as string;
            settings["WindSpeedUnit"] = WindSpeedUnitListPicker.SelectedItem as string;
            settings["PrecipitationUnit"] = PrecipitationUnitListPicker.SelectedItem as string;
            settings.Save();

            if (BackgroundUpdateToggleSwitch.IsChecked != true || LiveTileToggleSwitch.IsChecked != true)
                TileHelper.ResetTile();
        }

        private void ApplyToggleDependencies()
        {
            var master = BackgroundUpdateToggleSwitch.IsChecked == true;
            var liveTile = LiveTileToggleSwitch.IsChecked == true;

            LiveTileToggleSwitch.IsEnabled = master;
            NotificationToggleSwitch.IsEnabled = master;
            MonochromeTileToggleSwitch.IsEnabled = master && liveTile;
        }

        private void MasterToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (suppressToggleEvents) return;

            if (LiveTileToggleSwitch.IsChecked != true && NotificationToggleSwitch.IsChecked != true)
            {
                suppressToggleEvents = true;
                LiveTileToggleSwitch.IsChecked = true;
                suppressToggleEvents = false;
            }

            ApplyToggleDependencies();
        }

        private void MasterToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (suppressToggleEvents) return;
            ApplyToggleDependencies();
        }

        private void ChildToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (suppressToggleEvents) return;
            ApplyToggleDependencies();
        }

        private void ChildToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (suppressToggleEvents) return;

            if (LiveTileToggleSwitch.IsChecked != true && NotificationToggleSwitch.IsChecked != true)
            {
                suppressToggleEvents = true;
                BackgroundUpdateToggleSwitch.IsChecked = false;
                suppressToggleEvents = false;
            }

            ApplyToggleDependencies();
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