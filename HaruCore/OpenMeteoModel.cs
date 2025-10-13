using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace HaruCore
{
    public class GeocodingResponse
    {
        [JsonProperty("results")]
        public List<Location> Location { get; set; }

        public List<LocationRecord> ToLocationRecords()
        {
            return (Location ?? new List<Location>()).Select(l => new LocationRecord
            {
                NameShort = string.Format("{0}, {1}", l.Name, l.CountryCode),
                NameLong = string.Join(", ", new[] { l.Name }.Concat(new[] { l.Admin1, l.Admin2, l.Admin3, l.Admin4 }.Where(a => !string.IsNullOrWhiteSpace(a))).Concat(string.IsNullOrWhiteSpace(l.Country) ? new string[0] : new[] { l.Country })),
                Coordinates = string.Format("{0}, {1}", l.Latitude, l.Longitude),
                Latitude = l.Latitude,
                Longitude = l.Longitude
            }).ToList();
        }
    }

    public class Location
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("latitude")] public double Latitude { get; set; }
        [JsonProperty("longitude")] public double Longitude { get; set; }
        [JsonProperty("country_code")] public string CountryCode { get; set; }
        [JsonProperty("country")] public string Country { get; set; }
        [JsonProperty("admin1")] public string Admin1 { get; set; }
        [JsonProperty("admin2")] public string Admin2 { get; set; }
        [JsonProperty("admin3")] public string Admin3 { get; set; }
        [JsonProperty("admin4")] public string Admin4 { get; set; }
    }

    public class LocationRecord
    {
        public string NameShort { get; set; }
        public string NameLong { get; set; }
        public string Coordinates { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class ForecastResponse
    {
        [JsonProperty("current_units")] public CurrentUnits CurrentUnits { get; set; }
        [JsonProperty("current")] public Current Current { get; set; }
        [JsonProperty("hourly_units")] public HourlyUnits HourlyUnits { get; set; }
        [JsonProperty("hourly")] public Hourly Hourly { get; set; }
        [JsonProperty("daily_units")] public DailyUnits DailyUnits { get; set; }
        [JsonProperty("daily")] public Daily Daily { get; set; }

        public CurrentRecord ToCurrentRecord()
        {
            var c = Current;
            var cu = CurrentUnits;
            return new CurrentRecord
            {
                WeatherIcon = UnitHelper.GetWeatherIcon(c.WeatherCode, c.IsDay),
                WeatherTile = UnitHelper.GetWeatherTileIcon(c.WeatherCode, c.IsDay),
                WeatherDescription = UnitHelper.GetWeatherDescription(c.WeatherCode, c.IsDay),
                Temperature = string.Format("{0}{1}", Math.Ceiling(c.Temperature), cu.Temperature),
                ApparentTemperature = string.Format("{0}{1}", Math.Ceiling(c.ApparentTemperature), cu.ApparentTemperature),
                Humidity = c.RelativeHumidity + "%",
                Precipitation = string.Format("{0} {1}", c.Precipitation, cu.Precipitation),
                WindSpeed = string.Format("{0} {1}", c.WindSpeed, cu.WindSpeed),
                WindDirection = UnitHelper.InterpretDirection(c.WindDirection, false),
                Pressure = string.Format("{0} {1}", c.Pressure, cu.Pressure),
                Time = UnitHelper.InterpretTimeDifference(c.Time)
            };
        }

        public List<HourlyRecord> ToHourlyRecords()
        {
            return (Hourly == null || Hourly.Time == null ? new List<string>() : Hourly.Time).Select((t, i) =>
            {
                var dt = DateTime.Parse(t, null, DateTimeStyles.RoundtripKind);
                return new HourlyRecord
                {
                    Time = string.Format("{0} {1}", dt.ToString("ddd", CultureInfo.CurrentCulture), dt.ToString("t", CultureInfo.CurrentCulture)).ToUpper(),
                    WeatherIcon = UnitHelper.GetWeatherIcon(Hourly.WeatherCode.ElementAtOrDefault(i), Hourly.IsDay.ElementAtOrDefault(i)),
                    WeatherDescription = UnitHelper.GetWeatherDescription(Hourly.WeatherCode.ElementAtOrDefault(i), Hourly.IsDay.ElementAtOrDefault(i)),
                    Temperature = Math.Ceiling(Hourly.Temperature.ElementAtOrDefault(i)) + HourlyUnits.Temperature,
                    Humidity = Hourly.RelativeHumidity.ElementAtOrDefault(i) + "%",
                    Precipitation = Hourly.PrecipitationProbability.ElementAtOrDefault(i) + "%",
                    Wind = string.Format("{0} {1} {2}", Hourly.WindSpeed.ElementAtOrDefault(i), HourlyUnits.WindSpeed, UnitHelper.InterpretDirection(Hourly.WindDirection.ElementAtOrDefault(i), true))
                };
            }).ToList();
        }

        public List<DailyRecord> ToDailyRecords()
        {
            return (Daily == null || Daily.Time == null ? new List<string>() : Daily.Time).Select((t, i) => new DailyRecord
            {
                Time = DateTime.Parse(t).ToString("ddd M/dd", CultureInfo.CurrentCulture).ToUpper(),
                WeatherIcon = UnitHelper.GetWeatherIcon(Daily.WeatherCode.ElementAtOrDefault(i), true),
                WeatherDescription = UnitHelper.GetWeatherDescription(Daily.WeatherCode.ElementAtOrDefault(i), true),
                Temperature = string.Format("{0}°/{1}{2}", Math.Ceiling(Daily.TemperatureMax.ElementAtOrDefault(i)), Math.Ceiling(Daily.TemperatureMin.ElementAtOrDefault(i)), DailyUnits.TemperatureMin),
                Humidity = Daily.RelativeHumidityMean.ElementAtOrDefault(i) + "%",
                Precipitation = Daily.PrecipitationProbabilityMax.ElementAtOrDefault(i) + "%",
                Wind = string.Format("{0} {1} {2}", Daily.WindSpeedMax.ElementAtOrDefault(i), DailyUnits.WindSpeedMax, UnitHelper.InterpretDirection(Daily.WindDirectionDominant.ElementAtOrDefault(i), true))
            }).ToList();
        }
    }

    public class CurrentUnits
    {
        [JsonProperty("temperature_2m")] public string Temperature { get; set; }
        [JsonProperty("apparent_temperature")] public string ApparentTemperature { get; set; }
        [JsonProperty("precipitation")] public string Precipitation { get; set; }
        [JsonProperty("pressure_msl")] public string Pressure { get; set; }
        [JsonProperty("wind_speed_10m")] public string WindSpeed { get; set; }
    }

    public class Current
    {
        [JsonProperty("time")] public string Time { get; set; }
        [JsonProperty("temperature_2m")] public double Temperature { get; set; }
        [JsonProperty("relative_humidity_2m")] public int RelativeHumidity { get; set; }
        [JsonProperty("apparent_temperature")] public double ApparentTemperature { get; set; }
        [JsonProperty("is_day")] public bool IsDay { get; set; }
        [JsonProperty("precipitation")] public double Precipitation { get; set; }
        [JsonProperty("weather_code")] public int WeatherCode { get; set; }
        [JsonProperty("pressure_msl")] public double Pressure { get; set; }
        [JsonProperty("wind_speed_10m")] public double WindSpeed { get; set; }
        [JsonProperty("wind_direction_10m")] public int WindDirection { get; set; }
    }

    public class CurrentRecord
    {
        public string WeatherIcon { get; set; }
        public string WeatherTile { get; set; }
        public string WeatherDescription { get; set; }
        public string Temperature { get; set; }
        public string ApparentTemperature { get; set; }
        public string Humidity { get; set; }
        public string Precipitation { get; set; }
        public string WindSpeed { get; set; }
        public string WindDirection { get; set; }
        public string Pressure { get; set; }
        public string Time { get; set; }
    }

    public class HourlyUnits
    {
        [JsonProperty("temperature_2m")] public string Temperature { get; set; }
        [JsonProperty("wind_speed_10m")] public string WindSpeed { get; set; }
    }

    public class Hourly
    {
        [JsonProperty("time")] public List<string> Time { get; set; }
        [JsonProperty("temperature_2m")] public List<double> Temperature { get; set; }
        [JsonProperty("relative_humidity_2m")] public List<int> RelativeHumidity { get; set; }
        [JsonProperty("precipitation_probability")] public List<int> PrecipitationProbability { get; set; }
        [JsonProperty("weather_code")] public List<int> WeatherCode { get; set; }
        [JsonProperty("wind_speed_10m")] public List<double> WindSpeed { get; set; }
        [JsonProperty("wind_direction_10m")] public List<int> WindDirection { get; set; }
        [JsonProperty("is_day")] public List<bool> IsDay { get; set; }
    }

    public class HourlyRecord
    {
        public string Time { get; set; }
        public string WeatherIcon { get; set; }
        public string WeatherDescription { get; set; }
        public string Temperature { get; set; }
        public string Humidity { get; set; }
        public string Precipitation { get; set; }
        public string Wind { get; set; }
    }

    public class DailyUnits
    {
        [JsonProperty("temperature_2m_max")] public string TemperatureMax { get; set; }

        [JsonProperty("temperature_2m_min")] public string TemperatureMin { get; set; }
        [JsonProperty("wind_speed_10m_max")] public string WindSpeedMax { get; set; }
    }

    public class Daily
    {
        [JsonProperty("time")] public List<string> Time { get; set; }
        [JsonProperty("weather_code")] public List<int> WeatherCode { get; set; }
        [JsonProperty("temperature_2m_max")] public List<double> TemperatureMax { get; set; }
        [JsonProperty("temperature_2m_min")] public List<double> TemperatureMin { get; set; }
        [JsonProperty("precipitation_probability_max")] public List<int> PrecipitationProbabilityMax { get; set; }
        [JsonProperty("wind_speed_10m_max")] public List<double> WindSpeedMax { get; set; }
        [JsonProperty("wind_direction_10m_dominant")] public List<int> WindDirectionDominant { get; set; }
        [JsonProperty("relative_humidity_2m_mean")] public List<int> RelativeHumidityMean { get; set; }
    }

    public class DailyRecord
    {
        public string Time { get; set; }
        public string WeatherIcon { get; set; }
        public string WeatherDescription { get; set; }
        public string Temperature { get; set; }
        public string Humidity { get; set; }
        public string Precipitation { get; set; }
        public string Wind { get; set; }
    }
}