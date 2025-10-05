using Newtonsoft.Json;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Text;

namespace HaruCore
{
    public class OpenMeteoClient
    {
        public void SearchLocation(string query,
                                   Action<GeocodingResponse, Exception> callback,
                                   int count = 10)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                if (callback != null) callback(null, new ArgumentException("query empty"));
                return;
            }

            string url = string.Format(
                "http://geocoding-api.open-meteo.com/v1/search?name={0}&count={1}&language=en",
                Uri.EscapeDataString(query),
                count);

            WebClient wc = new WebClient();
            wc.DownloadStringCompleted += (s, e) =>
            {
                if (e.Error != null)
                {
                    if (callback != null) callback(null, e.Error);
                    return;
                }
                try
                {
                    GeocodingResponse geo = JsonConvert.DeserializeObject<GeocodingResponse>(e.Result);
                    if (callback != null) callback(geo, null);
                }
                catch (Exception ex)
                {
                    if (callback != null) callback(null, ex);
                }
            };
            wc.DownloadStringAsync(new Uri(url));
        }

        public void GetForecast(double latitude,
                                double longitude,
                                string temperatureUnit,
                                string windSpeedUnit,
                                string precipitationUnit,
                                Action<ForecastResponse, Exception> callback,
                                string timeFormat = "iso8601",
                                int forecastDays = 7,
                                int forecastHours = 12)
        {
            string lat = latitude.ToString(CultureInfo.InvariantCulture);
            string lon = longitude.ToString(CultureInfo.InvariantCulture);
            string hourly = "temperature_2m,relative_humidity_2m,precipitation_probability,weather_code,wind_speed_10m,wind_direction_10m,is_day";
            string daily = "weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max,wind_speed_10m_max,wind_direction_10m_dominant,relative_humidity_2m_mean";
            string current = "temperature_2m,relative_humidity_2m,apparent_temperature,is_day,precipitation,weather_code,pressure_msl,wind_speed_10m,wind_direction_10m";
            string timeZone = DateTimeZoneProviders.Tzdb.GetSystemDefault().Id;
            string url = string.Format(
                "http://api.open-meteo.com/v1/forecast?latitude={0}&longitude={1}&hourly={2}&daily={3}&current={4}&temperature_unit={5}&wind_speed_unit={6}&precipitation_unit={7}&timeformat={8}&timezone={9}&forecast_days={10}&forecast_hours={11}",
                lat,
                lon,
                hourly,
                daily,
                current,
                temperatureUnit,
                windSpeedUnit,
                precipitationUnit,
                timeFormat,
                timeZone,
                forecastDays,
                forecastHours);

            WebClient wc = new WebClient();
            wc.DownloadStringCompleted += (s, e) =>
            {
                if (e.Error != null)
                {
                    string cache = LoadFromIsolatedStorage("forecast.json");
                    if (cache != null)
                    {
                        try
                        {
                            ForecastResponse forecast = JsonConvert.DeserializeObject<ForecastResponse>(cache);
                            if (callback != null) callback(forecast, e.Error);
                            return;
                        }
                        catch (Exception) { }
                    }

                    if (callback != null) callback(null, e.Error);
                    return;
                }

                try
                {
                    ForecastResponse forecast = JsonConvert.DeserializeObject<ForecastResponse>(e.Result);
                    SaveToIsolatedStorage("forecast.json", e.Result);
                    if (callback != null) callback(forecast, null);
                }
                catch (Exception ex)
                {
                    if (callback != null) callback(null, ex);
                }
            };
            wc.DownloadStringAsync(new Uri(url));
        }

        private void SaveToIsolatedStorage(string fileName, string content)
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var stream = new IsolatedStorageFileStream(fileName, FileMode.Create, store))
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write(content);
                    }
                }
            }
        }

        private string LoadFromIsolatedStorage(string fileName)
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists(fileName))
                    return null;


                using (var stream = new IsolatedStorageFileStream(fileName, FileMode.Open, store))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
    }
}
