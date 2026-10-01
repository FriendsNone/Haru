using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Threading;

namespace HaruCore
{
    public class OpenMeteoClient
    {
        private const string CacheFileName = "forecast.json";

        public TimeSpan RequestTimeout { get; set; }

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
                "auto",
                forecastDays, forecastHours);

            DownloadString(url, (json, error) =>
            {
                ForecastResponse forecast = null;
                if (error == null)
                {
                    forecast = ParseForecast(json, out error);
                    if (forecast != null) SaveToCache(json);
                }

                if (forecast == null)
                {
                    Exception cacheError;
                    forecast = ParseForecast(LoadFromCache(), out cacheError);
                }

                InvokeCallback(callback, forecast, error);
            });
        }

        private void DownloadJson<T>(string url, Action<T, Exception> callback) where T : class
        {
            DownloadString(url, (json, error) =>
            {
                T result = null;
                if (error == null)
                {
                    try { result = JsonConvert.DeserializeObject<T>(json); }
                    catch (Exception ex) { error = ex; }
                }

                InvokeCallback(callback, result, error);
            });
        }

        private void DownloadString(string url, Action<string, Exception> completed)
        {
            var wc = new WebClient();
            Timer timer = null;
            var finished = 0;

            Action<string, Exception> finish = (result, error) =>
            {
                if (Interlocked.Exchange(ref finished, 1) != 0) return;
                if (timer != null) timer.Dispose();
                completed(result, error);
            };

            wc.DownloadStringCompleted += (s, e) =>
            {
                if (e.Error != null) finish(null, e.Error);
                else if (e.Cancelled) finish(null, new WebException("The request was cancelled."));
                else finish(e.Result, null);
            };

            if (RequestTimeout > TimeSpan.Zero)
            {
                timer = new Timer(state =>
                {
                    finish(null, new TimeoutException("The request timed out."));
                    try { wc.CancelAsync(); }
                    catch { }
                }, null, (int)RequestTimeout.TotalMilliseconds, Timeout.Infinite);
            }

            try
            {
                wc.DownloadStringAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                finish(null, ex);
            }
        }

        private static ForecastResponse ParseForecast(string json, out Exception error)
        {
            error = null;
            if (json == null) return null;

            try
            {
                var forecast = JsonConvert.DeserializeObject<ForecastResponse>(json);
                if (IsComplete(forecast)) return forecast;
                error = new FormatException("The forecast response is incomplete.");
            }
            catch (Exception ex)
            {
                error = ex;
            }
            return null;
        }

        private static bool IsComplete(ForecastResponse forecast)
        {
            return forecast != null
                && forecast.Current != null && forecast.CurrentUnits != null && !string.IsNullOrEmpty(forecast.Current.Time)
                && forecast.Hourly != null && forecast.HourlyUnits != null
                && forecast.Daily != null && forecast.DailyUnits != null;
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