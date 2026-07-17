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
                NameLong = BuildLongName(l),
                Coordinates = string.Format("{0}, {1}", l.Latitude, l.Longitude),
                Latitude = l.Latitude,
                Longitude = l.Longitude
            }).ToList();
        }

        // "Name, <non-blank admin regions>, Country" — omitting any parts that are blank.
        private static string BuildLongName(Location l)
        {
            var parts = new List<string> { l.Name };
            parts.AddRange(new[] { l.Admin1, l.Admin2, l.Admin3, l.Admin4 }
                .Where(a => !string.IsNullOrWhiteSpace(a)));
            if (!string.IsNullOrWhiteSpace(l.Country))
                parts.Add(l.Country);

            return string.Join(", ", parts);
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
        [JsonProperty("utc_offset_seconds")] public int UtcOffsetSeconds { get; set; }
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
                Time = UnitHelper.InterpretTimeDifference(c.Time, UtcOffsetSeconds)
            };
        }

        public List<HourlyRecord> ToHourlyRecords()
        {
            var records = new List<HourlyRecord>();
            if (Hourly == null || Hourly.Time == null) return records;

            var h = Hourly;
            var units = HourlyUnits;
            for (int i = 0; i < h.Time.Count; i++)
            {
                var dt = DateTime.Parse(h.Time[i], null, DateTimeStyles.RoundtripKind);
                int weatherCode = h.WeatherCode[i];
                bool isDay = h.IsDay[i];

                records.Add(new HourlyRecord
                {
                    Time = string.Format("{0} {1}", dt.ToString("ddd", CultureInfo.CurrentCulture), dt.ToString("t", CultureInfo.CurrentCulture)).ToUpper(),
                    WeatherIcon = UnitHelper.GetWeatherIcon(weatherCode, isDay),
                    WeatherDescription = UnitHelper.GetWeatherDescription(weatherCode, isDay),
                    Temperature = Math.Ceiling(h.Temperature[i]) + units.Temperature,
                    Humidity = h.RelativeHumidity[i] + "%",
                    Precipitation = h.PrecipitationProbability[i] + "%",
                    Wind = string.Format("{0} {1} {2}", h.WindSpeed[i], units.WindSpeed, UnitHelper.InterpretDirection(h.WindDirection[i], true))
                });
            }
            return records;
        }

        public List<DailyRecord> ToDailyRecords()
        {
            var records = new List<DailyRecord>();
            if (Daily == null || Daily.Time == null) return records;

            var d = Daily;
            var units = DailyUnits;
            for (int i = 0; i < d.Time.Count; i++)
            {
                int weatherCode = d.WeatherCode[i];

                records.Add(new DailyRecord
                {
                    Time = DateTime.Parse(d.Time[i]).ToString("ddd M/dd", CultureInfo.CurrentCulture).ToUpper(),
                    WeatherIcon = UnitHelper.GetWeatherIcon(weatherCode, true),
                    WeatherDescription = UnitHelper.GetWeatherDescription(weatherCode, true),
                    Temperature = string.Format("{0}°/{1}{2}", Math.Ceiling(d.TemperatureMax[i]), Math.Ceiling(d.TemperatureMin[i]), units.TemperatureMin),
                    Humidity = d.RelativeHumidityMean[i] + "%",
                    Precipitation = d.PrecipitationProbabilityMax[i] + "%",
                    Wind = string.Format("{0} {1} {2}", d.WindSpeedMax[i], units.WindSpeedMax, UnitHelper.InterpretDirection(d.WindDirectionDominant[i], true))
                });
            }
            return records;
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