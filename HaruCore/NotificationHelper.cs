using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
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

        public static void MaybeNotify(IList<NotificationCandidate> candidates, IEnumerable<string> activeKeys,
                                       string temperatureUnit)
        {
            if (!BaselineMutex.WaitOne(MutexTimeoutMilliseconds)) return;
            try
            {
                var state = LoadState() ?? new NotificationState();
                var last = state.Baselines ?? new Dictionary<string, NotificationBaseline>();
                var baselines = activeKeys.Distinct().Where(last.ContainsKey).ToDictionary(k => k, k => last[k]);

#if DEBUG
                if (candidates.Count > 0)
                {
                    ShowToast(candidates[0]);
                    state.LastToastUtc = DateTime.UtcNow;
                }
                foreach (var candidate in candidates)
                    baselines[candidate.Place.Key] = ToBaseline(candidate, temperatureUnit);
#else
                foreach (var candidate in candidates)
                    MaybeNotify(state, baselines, candidate, temperatureUnit);
#endif

                state.Baselines = baselines;
                SaveState(state);
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

        private static void MaybeNotify(NotificationState state, Dictionary<string, NotificationBaseline> baselines,
                                        NotificationCandidate candidate, string temperatureUnit)
        {
            var key = candidate.Place.Key;
            var next = ToBaseline(candidate, temperatureUnit);

            NotificationBaseline last;
            var comparable = baselines.TryGetValue(key, out last) && last != null
                && string.Equals(last.TemperatureUnit, next.TemperatureUnit, StringComparison.OrdinalIgnoreCase);

            if (comparable)
            {
                var precipitationChanged = GetPrecipitationLevel(last.Category) != GetPrecipitationLevel(next.Category);
                var tempJumped = Math.Abs(next.Temperature - last.Temperature) >= TemperatureThreshold(next.TemperatureUnit);

                if (!precipitationChanged && !tempJumped)
                    return;

                if (state.LastToastUtc.HasValue && DateTime.UtcNow - state.LastToastUtc.Value < Cooldown)
                    return;

                ShowToast(candidate);
                state.LastToastUtc = DateTime.UtcNow;
            }

            baselines[key] = next;
        }

        private static NotificationBaseline ToBaseline(NotificationCandidate candidate, string temperatureUnit)
        {
            return new NotificationBaseline
            {
                Temperature = candidate.Temperature,
                Category = UnitHelper.GetWeatherCategory(candidate.WeatherCode),
                TemperatureUnit = temperatureUnit
            };
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

        private static void ShowToast(NotificationCandidate candidate)
        {
            var toast = new ShellToast
            {
                Title = candidate.Place.Name,
                Content = string.Format("{0}  {1}", candidate.Current.Temperature, candidate.Current.WeatherDescription),
                NavigationUri = candidate.NavigationUri
            };
            toast.Show();
        }

        private static NotificationState LoadState()
        {
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!store.FileExists(BaselineFileName)) return null;
                    using (var stream = new IsolatedStorageFileStream(BaselineFileName, FileMode.Open, store))
                    using (var reader = new StreamReader(stream))
                    {
                        return JsonConvert.DeserializeObject<NotificationState>(reader.ReadToEnd());
                    }
                }
            }
            catch { return null; }
        }

        private static void SaveState(NotificationState state)
        {
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                using (var stream = new IsolatedStorageFileStream(BaselineFileName, FileMode.Create, store))
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(JsonConvert.SerializeObject(state));
                }
            }
            catch { }
        }
    }

    public class NotificationCandidate
    {
        public Place Place { get; set; }
        public CurrentRecord Current { get; set; }
        public double Temperature { get; set; }
        public int WeatherCode { get; set; }
        public Uri NavigationUri { get; set; }
    }

    public class NotificationState
    {
        public DateTime? LastToastUtc { get; set; }
        public Dictionary<string, NotificationBaseline> Baselines { get; set; }
    }

    public class NotificationBaseline
    {
        public double Temperature { get; set; }
        public string Category { get; set; }
        public string TemperatureUnit { get; set; }
    }
}
