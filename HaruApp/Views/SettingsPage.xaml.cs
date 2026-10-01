using HaruApp.Helpers;
using HaruApp.Resources;
using HaruCore;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Navigation;

namespace HaruApp.Views
{
    public partial class SettingsPage : PhoneApplicationPage
    {
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
            BackgroundUpdateToggleSwitch.IsChecked = HaruSettings.BackgroundUpdateEnabled;
            LiveTileToggleSwitch.IsChecked = HaruSettings.LiveTileEnabled;
            NotificationToggleSwitch.IsChecked = HaruSettings.NotificationEnabled;
            MonochromeTileToggleSwitch.IsChecked = HaruSettings.MonochromeTileEnabled;
            suppressToggleEvents = false;
            ApplyToggleDependencies();

            TemperatureUnitListPicker.SelectedItem = HaruSettings.TemperatureUnit;
            WindSpeedUnitListPicker.SelectedItem = HaruSettings.WindSpeedUnit;
            PrecipitationUnitListPicker.SelectedItem = HaruSettings.PrecipitationUnit;
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
            if (OpenMeteoClient.ClearCache())
                PromptHelper.ShowAlert(
                    AppResources.SettingClearForecastSuccessTitle,
                    AppResources.SettingClearForecastSuccessMessage,
                    AppResources.PromptOkay);
            else
                PromptHelper.ShowAlert(
                    AppResources.SettingClearForecastFailedTitle,
                    AppResources.ProgressError,
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
            return BackgroundUpdateToggleSwitch.IsChecked != HaruSettings.BackgroundUpdateEnabled ||
                   LiveTileToggleSwitch.IsChecked != HaruSettings.LiveTileEnabled ||
                   NotificationToggleSwitch.IsChecked != HaruSettings.NotificationEnabled ||
                   MonochromeTileToggleSwitch.IsChecked != HaruSettings.MonochromeTileEnabled ||
                   TemperatureUnitListPicker.SelectedItem as string != HaruSettings.TemperatureUnit ||
                   WindSpeedUnitListPicker.SelectedItem as string != HaruSettings.WindSpeedUnit ||
                   PrecipitationUnitListPicker.SelectedItem as string != HaruSettings.PrecipitationUnit;
        }

        private void SaveSettings()
        {
            HaruSettings.BackgroundUpdateEnabled = BackgroundUpdateToggleSwitch.IsChecked == true;
            HaruSettings.LiveTileEnabled = LiveTileToggleSwitch.IsChecked == true;
            HaruSettings.NotificationEnabled = NotificationToggleSwitch.IsChecked == true;
            HaruSettings.MonochromeTileEnabled = MonochromeTileToggleSwitch.IsChecked == true;
            HaruSettings.TemperatureUnit = TemperatureUnitListPicker.SelectedItem as string;
            HaruSettings.WindSpeedUnit = WindSpeedUnitListPicker.SelectedItem as string;
            HaruSettings.PrecipitationUnit = PrecipitationUnitListPicker.SelectedItem as string;
            HaruSettings.Save();

            if (BackgroundUpdateToggleSwitch.IsChecked != true || LiveTileToggleSwitch.IsChecked != true)
                TileHelper.ResetTile();

            AgentHelper.StartPeriodicAgent();
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