using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading;

namespace HaruCore
{
    public static class NotificationHelper
    {
        private const string BaselineFileName = "notification.json";
        private const int MutexTimeoutMilliseconds = 5000;
        private static readonly Mutex BaselineMutex = new Mutex(false, "HaruNotificationBaseline");
        private static readonly TimeSpan Cooldown = TimeSpan.FromHours(2);

        private static readonly string[] LegacyKeys =
        {
            "LastNotifiedTemp", "LastNotifiedCategory", "LastNotifiedTempUnit", "LastNotifiedLocation"
        };

        public static void MaybeNotify(string location, CurrentRecord current,
                                       double temperature, int weatherCode, string temperatureUnit)
        {
            var next = new NotificationBaseline
            {
                Temperature = temperature,
                Category = UnitHelper.GetWeatherCategory(weatherCode),
                TemperatureUnit = temperatureUnit,
                Location = location
            };

            if (!BaselineMutex.WaitOne(MutexTimeoutMilliseconds)) return;
            try
            {
                MaybeNotify(LoadBaseline(), next, current);
            }
            finally
            {
                BaselineMutex.ReleaseMutex();
            }
        }

        public static void RemoveLegacyBaseline(IsolatedStorageSettings settings)
        {
            foreach (var key in LegacyKeys)
                settings.Remove(key);
        }

        private static void MaybeNotify(NotificationBaseline last, NotificationBaseline next, CurrentRecord current)
        {
#if DEBUG
            ShowToast(next.Location, current);
            next.LastToastUtc = DateTime.UtcNow;
            SaveBaseline(next);
#else
            var comparable = last != null
                && string.Equals(last.TemperatureUnit, next.TemperatureUnit, StringComparison.OrdinalIgnoreCase)
                && string.Equals(last.Location, next.Location, StringComparison.Ordinal);

            if (comparable)
            {
                var precipitationChanged = GetPrecipitationLevel(last.Category) != GetPrecipitationLevel(next.Category);
                var tempJumped = Math.Abs(next.Temperature - last.Temperature) >= TemperatureThreshold(next.TemperatureUnit);

                if (!precipitationChanged && !tempJumped)
                    return;

                if (last.LastToastUtc.HasValue && DateTime.UtcNow - last.LastToastUtc.Value < Cooldown)
                    return;

                ShowToast(next.Location, current);
                next.LastToastUtc = DateTime.UtcNow;
            }

            SaveBaseline(next);
#endif
        }

        private static int GetPrecipitationLevel(string category)
        {
            switch (category)
            {
                case "rain":
                case "sleet":
                case "snow":
                    return 1;
                case "thunderstorms":
                    return 2;
                default:
                    return 0;
            }
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

        private static NotificationBaseline LoadBaseline()
        {
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!store.FileExists(BaselineFileName)) return null;
                    using (var stream = new IsolatedStorageFileStream(BaselineFileName, FileMode.Open, store))
                    using (var reader = new StreamReader(stream))
                    {
                        return JsonConvert.DeserializeObject<NotificationBaseline>(reader.ReadToEnd());
                    }
                }
            }
            catch { return null; }
        }

        private static void SaveBaseline(NotificationBaseline baseline)
        {
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                using (var stream = new IsolatedStorageFileStream(BaselineFileName, FileMode.Create, store))
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(JsonConvert.SerializeObject(baseline));
                }
            }
            catch { }
        }
    }

    public class NotificationBaseline
    {
        public double Temperature { get; set; }
        public string Category { get; set; }
        public string TemperatureUnit { get; set; }
        public string Location { get; set; }
        public DateTime? LastToastUtc { get; set; }
    }
}
