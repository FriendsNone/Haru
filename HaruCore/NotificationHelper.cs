using Microsoft.Phone.Shell;
using System;
using System.IO.IsolatedStorage;

namespace HaruCore
{
    public static class NotificationHelper
    {
        private const string TempKey = "LastNotifiedTemp";
        private const string CategoryKey = "LastNotifiedCategory";
        private const string UnitKey = "LastNotifiedTempUnit";
        private const string LocationKey = "LastNotifiedLocation";

        public static void MaybeNotify(IsolatedStorageSettings settings, string location, CurrentRecord current,
                                       double temperature, int weatherCode, string temperatureUnit)
        {
            var category = UnitHelper.GetWeatherCategory(weatherCode);

#if DEBUG
            ShowToast(location, current);
            StoreBaseline(settings, location, temperature, category, temperatureUnit);
            return;
#else
            var hasBaseline = settings.Contains(TempKey) && settings.Contains(CategoryKey)
                              && settings.Contains(UnitKey) && settings.Contains(LocationKey);

            var comparable = hasBaseline
                && string.Equals((string)settings[UnitKey], temperatureUnit, StringComparison.OrdinalIgnoreCase)
                && string.Equals((string)settings[LocationKey], location, StringComparison.Ordinal);

            if (!comparable)
            {
                StoreBaseline(settings, location, temperature, category, temperatureUnit);
                return;
            }

            var lastTemp = Convert.ToDouble(settings[TempKey]);
            var lastCategory = (string)settings[CategoryKey];

            var categoryChanged = !string.Equals(lastCategory, category, StringComparison.Ordinal);
            var tempJumped = Math.Abs(temperature - lastTemp) >= TemperatureThreshold(temperatureUnit);

            if (!categoryChanged && !tempJumped)
                return;

            ShowToast(location, current);
            StoreBaseline(settings, location, temperature, category, temperatureUnit);
#endif
        }

        private static double TemperatureThreshold(string temperatureUnit)
        {
            return string.Equals(temperatureUnit, "fahrenheit", StringComparison.OrdinalIgnoreCase) ? 9.0 : 5.0;
        }

        private static void ShowToast(string location, CurrentRecord current)
        {
            var toast = new ShellToast
            {
                Title = location,
                Content = string.Format("{0}  {1}", current.Temperature, current.WeatherDescription),
                NavigationUri = new Uri("/Views/MainPage.xaml", UriKind.Relative)
            };
            toast.Show();
        }

        private static void StoreBaseline(IsolatedStorageSettings settings, string location, double temperature,
                                          string category, string temperatureUnit)
        {
            settings[TempKey] = temperature;
            settings[CategoryKey] = category;
            settings[UnitKey] = temperatureUnit;
            settings[LocationKey] = location;
            settings.Save();
        }
    }
}
