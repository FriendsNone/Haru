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
        public void SearchLocation(string query, Action<List<LocationResult>, Exception> callback, int count = 10)
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
            string hourly = "temperature_2m,relative_humidity_2m,dew_point_2m,apparent_temperature,precipitation_probability,precipitation,rain,showers,snowfall,snow_depth,weather_code,pressure_msl,surface_pressure,cloud_cover,cloud_cover_low,cloud_cover_mid,cloud_cover_high,visibility,evapotranspiration,et0_fao_evapotranspiration,vapour_pressure_deficit,wind_speed_10m,wind_speed_80m,wind_speed_120m,wind_speed_180m,wind_direction_10m,wind_direction_80m,wind_direction_120m,wind_direction_180m,wind_gusts_10m,temperature_80m,temperature_120m,temperature_180m,soil_temperature_0cm,soil_temperature_6cm,soil_temperature_18cm,soil_temperature_54cm,soil_moisture_0_to_1cm,soil_moisture_1_to_3cm,soil_moisture_3_to_9cm,soil_moisture_9_to_27cm,soil_moisture_27_to_81cm";
            string daily = "";
            string current = "temperature_2m,relative_humidity_2m,apparent_temperature,is_day,precipitation,rain,showers,snowfall,weather_code,cloud_cover,pressure_msl,surface_pressure,wind_speed_10m,wind_direction_10m,wind_gusts_10m";
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
