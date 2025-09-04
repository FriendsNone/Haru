using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HaruCore
{
    public class GeocodingResponse
    {
        [JsonProperty("results")]
        public List<LocationResult> Results { get; set; }
    }

    public class LocationResult
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("latitude")]
        public double Latitude { get; set; }
        [JsonProperty("longitude")]
        public double Longitude { get; set; }
        [JsonProperty("country_code")]
        public string CountryCode { get; set; }
        [JsonProperty("country")]
        public string Country { get; set; }
        [JsonProperty("admin1")]
        public string Admin1 { get; set; }
        [JsonProperty("admin2")]
        public string Admin2 { get; set; }
        [JsonProperty("admin3")]
        public string Admin3 { get; set; }
        [JsonProperty("admin4")]
        public string Admin4 { get; set; }

        public string LocationName
        {
            get
            {
                return string.Format("{0}, {1}, {2}",
                    Name,
                    Admin1,
                    Country);
            }
        }

        public string LocationCoordinates
        {
            get
            {
                return string.Format("{0}, {1}",
                    Latitude,
                    Longitude);
            }
        }
    }

    public class ForecastResponse
    {
        [JsonProperty("latitude")]
        public double Latitude { get; set; }
        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        [JsonProperty("current")]
        public CurrentWeather CurrentWeather { get; set; }

        //[JsonProperty("hourly")]
        //public Hourly? Hourly { get; set; }

        //[JsonProperty("hourly_units")]
        //public Daily? Daily { get; set; }
    }

    public class CurrentWeather
    {
        [JsonProperty("time")]
        public string Time { get; set; }
        [JsonProperty("interval")]
        public int Interval { get; set; }
        [JsonProperty("temperature_2m")]
        public double Temperature { get; set; }
        [JsonProperty("relative_humidity_2m")]
        public double RelativeHumidity { get; set; }
        [JsonProperty("apparent_temperature")]
        public double ApparentTemperature { get; set; }
        [JsonProperty("is_day")]
        public bool IsDay { get; set; }
        [JsonProperty("precipitation")]
        public double Precipitation { get; set; }
        //[JsonProperty("rain")]
        //public double Rain { get; set; }
        //[JsonProperty("showers")]
        //public double Showers { get; set; }
        //[JsonProperty("snowfall")]
        //public double Snowfall { get; set; }
        [JsonProperty("weather_code")]
        public int WeatherCode { get; set; }
        [JsonProperty("cloud_cover")]
        public double CloudCover { get; set; }
        [JsonProperty("pressure_msl")]
        public double Pressure { get; set; }
        [JsonProperty("surface_pressure")]
        public double SurfacePressure { get; set; }
        [JsonProperty("wind_speed_10m")]
        public double WindSpeed { get; set; }
        [JsonProperty("wind_direction_10m")]
        public double WindDirection { get; set; }
        [JsonProperty("wind_gusts_10m")]
        public double WindGusts { get; set; }
    }
}
