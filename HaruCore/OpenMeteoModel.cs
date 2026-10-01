using HaruCore.Resources;
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
                NameShort = string.IsNullOrWhiteSpace(l.CountryCode)
                    ? l.Name
                    : string.Format("{0}, {1}", l.Name, l.CountryCode),
                NameLong = BuildLongName(l),
                Coordinates = string.Format(CultureInfo.InvariantCulture, "{0:0.####}, {1:0.####}", l.Latitude, l.Longitude),
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

        // Shown in place of any value the API sent as null.
        private const string Placeholder = "—";

        public CurrentRecord ToCurrentRecord()
        {
            var c = Current;
            var cu = CurrentUnits;
            var isDay = c.IsDay ?? true;
            return new CurrentRecord
            {
                WeatherIcon = UnitHelper.GetWeatherIcon(c.WeatherCode, isDay),
                WeatherTile = UnitHelper.GetWeatherTileIcon(c.WeatherCode, isDay),
                WeatherDescription = UnitHelper.GetWeatherDescription(c.WeatherCode, isDay),
                Temperature = FormatTemperature(c.Temperature, cu.Temperature),
                ApparentTemperature = FormatTemperature(c.ApparentTemperature, cu.ApparentTemperature),
                Humidity = FormatValue(c.RelativeHumidity, "%"),
                Precipitation = FormatValue(c.Precipitation, " " + cu.Precipitation),
                WindSpeed = FormatValue(c.WindSpeed, " " + cu.WindSpeed),
                WindDirection = c.WindDirection.HasValue ? UnitHelper.InterpretDirection(c.WindDirection.Value, false) : Placeholder,
                Pressure = FormatValue(c.Pressure, " " + cu.Pressure),
                ObservedUtc = GetObservedUtc()
            };
        }

        private DateTime GetObservedUtc()
        {
            var local = DateTime.Parse(Current.Time, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            return DateTime.SpecifyKind(local - TimeSpan.FromSeconds(UtcOffsetSeconds), DateTimeKind.Utc);
        }

        private static double RoundTemperature(double temperature)
        {
            return Math.Sign(temperature) * Math.Floor(Math.Abs(temperature) + 0.5);
        }

        private static string FormatTemperature(double? temperature, string unit)
        {
            return temperature.HasValue ? RoundTemperature(temperature.Value) + unit : Placeholder;
        }

        private static string FormatValue<T>(T? value, string unit) where T : struct
        {
            return value.HasValue ? value.Value + unit : Placeholder;
        }

        private static string FormatWind(double? speed, string unit, int? direction)
        {
            if (!speed.HasValue) return Placeholder;

            var wind = speed.Value + " " + unit;
            return direction.HasValue ? wind + " " + UnitHelper.InterpretDirection(direction.Value, true) : wind;
        }

        // The value at index, or null when the list is missing, too short or holds null there.
        private static T? At<T>(List<T?> list, int index) where T : struct
        {
            return list != null && index < list.Count ? list[index] : null;
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
                var weatherCode = At(h.WeatherCode, i);
                var isDay = At(h.IsDay, i) ?? true;

                records.Add(new HourlyRecord
                {
                    Time = string.Format("{0} {1}", dt.ToString("ddd", CultureInfo.CurrentCulture), dt.ToString("t", CultureInfo.CurrentCulture)).ToUpper(),
                    WeatherIcon = UnitHelper.GetWeatherIcon(weatherCode, isDay),
                    WeatherDescription = UnitHelper.GetWeatherDescription(weatherCode, isDay),
                    Temperature = FormatTemperature(At(h.Temperature, i), units.Temperature),
                    Humidity = FormatValue(At(h.RelativeHumidity, i), "%"),
                    Precipitation = FormatValue(At(h.PrecipitationProbability, i), "%"),
                    Wind = FormatWind(At(h.WindSpeed, i), units.WindSpeed, At(h.WindDirection, i))
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
                var weatherCode = At(d.WeatherCode, i);
                var date = DateTime.Parse(d.Time[i], CultureInfo.InvariantCulture);

                records.Add(new DailyRecord
                {
                    Time = date.ToString(CoreResources.DailyDateFormat, CultureInfo.CurrentCulture).ToUpper(),
                    WeatherIcon = UnitHelper.GetWeatherIcon(weatherCode, true),
                    WeatherDescription = UnitHelper.GetWeatherDescription(weatherCode, true),
                    Temperature = FormatTemperature(At(d.TemperatureMax, i), "°") + "/" + FormatTemperature(At(d.TemperatureMin, i), units.TemperatureMin),
                    Humidity = FormatValue(At(d.RelativeHumidityMean, i), "%"),
                    Precipitation = FormatValue(At(d.PrecipitationProbabilityMax, i), "%"),
                    Wind = FormatWind(At(d.WindSpeedMax, i), units.WindSpeedMax, At(d.WindDirectionDominant, i))
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
        [JsonProperty("temperature_2m")] public double? Temperature { get; set; }
        [JsonProperty("relative_humidity_2m")] public int? RelativeHumidity { get; set; }
        [JsonProperty("apparent_temperature")] public double? ApparentTemperature { get; set; }
        [JsonProperty("is_day")] public bool? IsDay { get; set; }
        [JsonProperty("precipitation")] public double? Precipitation { get; set; }
        [JsonProperty("weather_code")] public int? WeatherCode { get; set; }
        [JsonProperty("pressure_msl")] public double? Pressure { get; set; }
        [JsonProperty("wind_speed_10m")] public double? WindSpeed { get; set; }
        [JsonProperty("wind_direction_10m")] public int? WindDirection { get; set; }
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
        public DateTime ObservedUtc { get; set; }
    }

    public class HourlyUnits
    {
        [JsonProperty("temperature_2m")] public string Temperature { get; set; }
        [JsonProperty("wind_speed_10m")] public string WindSpeed { get; set; }
    }

    public class Hourly
    {
        [JsonProperty("time")] public List<string> Time { get; set; }
        [JsonProperty("temperature_2m")] public List<double?> Temperature { get; set; }
        [JsonProperty("relative_humidity_2m")] public List<int?> RelativeHumidity { get; set; }
        [JsonProperty("precipitation_probability")] public List<int?> PrecipitationProbability { get; set; }
        [JsonProperty("weather_code")] public List<int?> WeatherCode { get; set; }
        [JsonProperty("wind_speed_10m")] public List<double?> WindSpeed { get; set; }
        [JsonProperty("wind_direction_10m")] public List<int?> WindDirection { get; set; }
        [JsonProperty("is_day")] public List<bool?> IsDay { get; set; }
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
        [JsonProperty("weather_code")] public List<int?> WeatherCode { get; set; }
        [JsonProperty("temperature_2m_max")] public List<double?> TemperatureMax { get; set; }
        [JsonProperty("temperature_2m_min")] public List<double?> TemperatureMin { get; set; }
        [JsonProperty("precipitation_probability_max")] public List<int?> PrecipitationProbabilityMax { get; set; }
        [JsonProperty("wind_speed_10m_max")] public List<double?> WindSpeedMax { get; set; }
        [JsonProperty("wind_direction_10m_dominant")] public List<int?> WindDirectionDominant { get; set; }
        [JsonProperty("relative_humidity_2m_mean")] public List<int?> RelativeHumidityMean { get; set; }
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