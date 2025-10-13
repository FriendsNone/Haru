using Newtonsoft.Json;
using NodaTime;
using System;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;

namespace HaruCore
{
    public class OpenMeteoClient
    {
        private const string CacheFileName = "forecast.json";

        public void SearchLocation(string query, Action<GeocodingResponse, Exception> callback, int count = 10)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                InvokeCallback(callback, null, new ArgumentException("query empty"));
                return;
            }

            var url = string.Format("http://geocoding-api.open-meteo.com/v1/search?name={0}&count={1}&language=en",
                Uri.EscapeDataString(query), count);

            DownloadJson<GeocodingResponse>(url, callback);
        }

        public void GetForecast(double latitude, double longitude, string temperatureUnit, string windSpeedUnit, 
            string precipitationUnit, Action<ForecastResponse, Exception> callback, string timeFormat = "iso8601", 
            int forecastDays = 7, int forecastHours = 12)
        {
            var url = string.Format(
                "http://api.open-meteo.com/v1/forecast?latitude={0}&longitude={1}&hourly={2}&daily={3}&current={4}&temperature_unit={5}&wind_speed_unit={6}&precipitation_unit={7}&timeformat={8}&timezone={9}&forecast_days={10}&forecast_hours={11}",
                latitude.ToString(CultureInfo.InvariantCulture),
                longitude.ToString(CultureInfo.InvariantCulture),
                "temperature_2m,relative_humidity_2m,precipitation_probability,weather_code,wind_speed_10m,wind_direction_10m,is_day",
                "weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max,wind_speed_10m_max,wind_direction_10m_dominant,relative_humidity_2m_mean",
                "temperature_2m,relative_humidity_2m,apparent_temperature,is_day,precipitation,weather_code,pressure_msl,wind_speed_10m,wind_direction_10m",
                temperatureUnit, windSpeedUnit, precipitationUnit, timeFormat,
                DateTimeZoneProviders.Tzdb.GetSystemDefault().Id,
                forecastDays, forecastHours);

            var wc = new WebClient();
            wc.DownloadStringCompleted += (s, e) =>
            {
                if (e.Error != null)
                {
                    var cache = LoadFromCache();
                    if (cache != null)
                    {
                        try
                        {
                            var forecast = JsonConvert.DeserializeObject<ForecastResponse>(cache);
                            InvokeCallback(callback, forecast, e.Error);
                            return;
                        }
                        catch { }
                    }
                    InvokeCallback(callback, null, e.Error);
                    return;
                }

                try
                {
                    var forecast = JsonConvert.DeserializeObject<ForecastResponse>(e.Result);
                    SaveToCache(e.Result);
                    InvokeCallback(callback, forecast, null);
                }
                catch (Exception ex)
                {
                    InvokeCallback(callback, null, ex);
                }
            };
            wc.DownloadStringAsync(new Uri(url));
        }

        private void DownloadJson<T>(string url, Action<T, Exception> callback) where T : class
        {
            var wc = new WebClient();
            wc.DownloadStringCompleted += (s, e) =>
            {
                if (e.Error != null)
                {
                    InvokeCallback(callback, null, e.Error);
                    return;
                }
                try
                {
                    var result = JsonConvert.DeserializeObject<T>(e.Result);
                    InvokeCallback(callback, result, null);
                }
                catch (Exception ex)
                {
                    InvokeCallback(callback, null, ex);
                }
            };
            wc.DownloadStringAsync(new Uri(url));
        }

        private void InvokeCallback<T>(Action<T, Exception> callback, T result, Exception error)
        {
            if (callback != null) callback(result, error);
        }

        private void SaveToCache(string content)
        {
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                using (var stream = new IsolatedStorageFileStream(CacheFileName, FileMode.Create, store))
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(content);
                }
            }
            catch { }
        }

        private string LoadFromCache()
        {
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!store.FileExists(CacheFileName)) return null;
                    using (var stream = new IsolatedStorageFileStream(CacheFileName, FileMode.Open, store))
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch { return null; }
        }
    }
}