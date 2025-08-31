using Newtonsoft.Json;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace HaruCore
{
    public class OpenMeteoClient
    {
        public void SearchLocation(string query, Action<List<LocationResult>, Exception> callback, int count = 5)
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
                    if (callback != null) callback(geo != null ? geo.Results : null, null);
                }
                catch (Exception ex)
                {
                    if (callback != null) callback(null, ex);
                }
            };
            wc.DownloadStringAsync(new Uri(url));
        }

        public void GetForecast(double latitude, double longitude, Action<ForecastResponse, Exception> callback)
        {
            string lat = latitude.ToString(CultureInfo.InvariantCulture);
            string lon = longitude.ToString(CultureInfo.InvariantCulture);
            string tz = DateTimeZoneProviders.Tzdb.GetSystemDefault().Id;
            string url = string.Format(
                "http://api.open-meteo.com/v1/forecast?latitude={0}&longitude={1}&current=temperature_2m,relative_humidity_2m,apparent_temperature,is_day,precipitation,rain,showers,snowfall,weather_code,cloud_cover,pressure_msl,surface_pressure,wind_speed_10m,wind_direction_10m,wind_gusts_10m&timezone={2}",
                lat,
                lon,
                tz);

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
                    ForecastResponse forecast = JsonConvert.DeserializeObject<ForecastResponse>(e.Result);
                    if (callback != null) callback(forecast, null);
                }
                catch (Exception ex)
                {
                    if (callback != null) callback(null, ex);
                }
            };
            wc.DownloadStringAsync(new Uri(url));
        }
    }
}
