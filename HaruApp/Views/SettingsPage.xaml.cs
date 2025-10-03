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
using System.ComponentModel;

namespace HaruApp.Views
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        private IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        private bool _isPromptShown = false;

        public SettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

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

            if (_isPromptShown)
                return;

            bool hasChanges =
                (settings.Contains("TemperatureUnit") && TemperatureUnitListPicker.SelectedItem as string != settings["TemperatureUnit"] as string) ||
                (settings.Contains("WindSpeedUnit") && WindSpeedUnitListPicker.SelectedItem as string != settings["WindSpeedUnit"] as string) ||
                (settings.Contains("PrecipitationUnit") && PrecipitationUnitListPicker.SelectedItem as string != settings["PrecipitationUnit"] as string);

            if (hasChanges)
            {
                e.Cancel = true;
                _isPromptShown = true;

                Dispatcher.BeginInvoke(() =>
                {
                    CustomMessageBox messageBox = new CustomMessageBox()
                    {
                        Caption = "Do you want to save changes?",
                        Message = "Your changes will be lost if you don't save them.",
                        LeftButtonContent = "save",
                        RightButtonContent = "don't save"
                    };

                    messageBox.Dismissed += (s1, e1) =>
                    {
                        switch (e1.Result)
                        {
                            case CustomMessageBoxResult.LeftButton:
                                SaveSettings();
                                NavigationService.GoBack();
                                break;
                            case CustomMessageBoxResult.RightButton:
                                NavigationService.GoBack();
                                break;
                            case CustomMessageBoxResult.None:
                            default:
                                _isPromptShown = false;
                                break;
                        }
                    };

                    messageBox.Show();
                });
            }
        }

        private void ClearSavedForecastButton_Click(object sender, RoutedEventArgs e)
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists("forecast.json"))
                    store.DeleteFile("forecast.json");
            }

            CustomMessageBox messageBox = new CustomMessageBox()
            {
                Caption = "Saved forecast cleared!",
                Message = "A fresh and up-to-date forecast will be ready the next time you refresh or open the app.",
                LeftButtonContent = "okay",
            };

            messageBox.Show();
        }

        private void SaveApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            SaveSettings();
            NavigationService.GoBack();
        }

        private void CancelApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }

        private void SaveSettings()
        {
            settings["TemperatureUnit"] = TemperatureUnitListPicker.SelectedItem as string;
            settings["WindSpeedUnit"] = WindSpeedUnitListPicker.SelectedItem as string;
            settings["PrecipitationUnit"] = PrecipitationUnitListPicker.SelectedItem as string;
            settings.Save();
        }
    }
}