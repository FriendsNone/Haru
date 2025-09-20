using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        [JsonProperty("current_units")]
        public CurrentWeatherUnits CurrentUnits { get; set; }

        [JsonProperty("current")]
        public CurrentWeather Current { get; set; }

        [JsonProperty("hourly_units")]
        public HourlyWeatherUnits HourlyUnits { get; set; }

        [JsonProperty("hourly")]
        public HourlyWeather Hourly { get; set; }

        [JsonProperty("daily_units")]
        public DailyWeatherUnits DailyUnits { get; set; }

        [JsonProperty("daily")]
        public DailyWeather Daily { get; set; }

        public CurrentRecord ToCurrentRecord()
        {
            var records = new CurrentRecord{
                WeatherIcon         = WeatherInterpretationModel.GetWeatherIcon(this.Current.WeatherCode, this.Current.IsDay),
                WeatherDescription  = WeatherInterpretationModel.GetWeatherDescription(this.Current.WeatherCode, this.Current.IsDay),
                Temperature         = string.Format("{0}{1}", Math.Ceiling(this.Current.Temperature), this.CurrentUnits.Temperature),
                ApparentTemperature = string.Format("{0}{1}", Math.Ceiling(this.Current.Temperature), this.CurrentUnits.ApparentTemperature),
                Humidity            = string.Format("{0}%", this.Current.RelativeHumidity),
                Precipitation       = string.Format("{0} {1}", this.Current.Precipitation, this.CurrentUnits.Precipitation), 
                WindSpeed           = string.Format("{0} {1}", this.Current.WindSpeed, this.CurrentUnits.WindSpeed),
                WindDirection       = UnitModel.InterpretDirection(this.Current.WindDirection, false),
                Pressure            = string.Format("{0} {1}", this.Current.Pressure, this.CurrentUnits.Pressure),
                Time                = string.Format("Forecast as of {0}", UnitModel.InterpretTimeDifference(this.Current.Time))
            };

            return records;
        }

        public List<HourlyRecord> ToHourlyRecords()
        {
            var records = new List<HourlyRecord>();
            for (int i = 0; i < this.Hourly.Time.Count; i++)
            {
                DateTime time = DateTime.Parse(this.Hourly.Time[i], null, DateTimeStyles.RoundtripKind);

                records.Add(new HourlyRecord
                {
                    Time               = string.Format("{0} {1}", time.ToString("ddd", CultureInfo.CurrentCulture), time.ToString("t", CultureInfo.CurrentCulture)).ToUpper(),
                    WeatherIcon        = WeatherInterpretationModel.GetWeatherIcon(this.Hourly.WeatherCode.ElementAtOrDefault(i), this.Hourly.IsDay.ElementAtOrDefault(i)),
                    WeatherDescription = WeatherInterpretationModel.GetWeatherDescription(this.Hourly.WeatherCode.ElementAtOrDefault(i), this.Hourly.IsDay.ElementAtOrDefault(i)),
                    Temperature        = string.Format("{0}°", Math.Ceiling(this.Hourly.Temperature.ElementAtOrDefault(i))),
                    Humidity           = string.Format("{0}%", this.Hourly.RelativeHumidity.ElementAtOrDefault(i)),
                    Precipitation      = string.Format("{0}%", this.Hourly.PrecipitationProbability.ElementAtOrDefault(i)),
                    Wind               = string.Format("{0} {1} {2}", this.Hourly.WindSpeed.ElementAtOrDefault(i), this.HourlyUnits.WindSpeed, UnitModel.InterpretDirection(this.Hourly.WindDirection.ElementAtOrDefault(i), true)),
                });
            }
            return records;
        }

        public List<DailyRecord> ToDailyRecords()
        {
            var records = new List<DailyRecord>();
            for (int i = 0; i < this.Daily.Time.Count; i++)
            {
                records.Add(new DailyRecord
                {
                    Time               = DateTime.Parse(this.Daily.Time[i]).ToString("ddd M/dd", CultureInfo.CurrentCulture).ToUpper(),
                    WeatherIcon        = WeatherInterpretationModel.GetWeatherIcon(this.Daily.WeatherCode.ElementAtOrDefault(i), true),
                    WeatherDescription = WeatherInterpretationModel.GetWeatherDescription(this.Daily.WeatherCode.ElementAtOrDefault(i), true),
                    Temperature        = string.Format("{0}°/{1}°", Math.Ceiling(this.Daily.TemperatureMax.ElementAtOrDefault(i)), Math.Ceiling(this.Daily.TemperatureMin.ElementAtOrDefault(i))),
                    Humidity           = string.Format("{0}%", this.Daily.RelativeHumidityMean.ElementAtOrDefault(i)),
                    Precipitation      = string.Format("{0}%", this.Daily.PrecipitationProbabilityMax.ElementAtOrDefault(i)),
                    Wind               = string.Format("{0} {1} {2}", this.Daily.WindSpeedMax.ElementAtOrDefault(i), this.DailyUnits.WindSpeedMax, UnitModel.InterpretDirection(this.Daily.WindDirectionDominant.ElementAtOrDefault(i), true)),
                });
            }
            return records;
        }
    }

    public class CurrentWeatherUnits
    {
        [JsonProperty("temperature_2m")]
        public string Temperature { get; set; }

        [JsonProperty("apparent_temperature")]
        public string ApparentTemperature { get; set; }

        [JsonProperty("precipitation")]
        public string Precipitation { get; set; }

        [JsonProperty("pressure_msl")]
        public string Pressure { get; set; }

        [JsonProperty("wind_speed_10m")]
        public string WindSpeed { get; set; }
    }

    public class CurrentWeather
    {
        [JsonProperty("time")]
        public string Time { get; set; }

        [JsonProperty("temperature_2m")]
        public double Temperature { get; set; }

        [JsonProperty("relative_humidity_2m")]
        public int RelativeHumidity { get; set; }

        [JsonProperty("apparent_temperature")]
        public double ApparentTemperature { get; set; }

        [JsonProperty("is_day")]
        public bool IsDay { get; set; }

        [JsonProperty("precipitation")]
        public double Precipitation { get; set; }

        [JsonProperty("weather_code")]
        public int WeatherCode { get; set; }

        [JsonProperty("pressure_msl")]
        public double Pressure { get; set; }

        [JsonProperty("wind_speed_10m")]
        public double WindSpeed { get; set; }

        [JsonProperty("wind_direction_10m")]
        public int WindDirection { get; set; }
    }

    public class CurrentRecord
    {
        public string WeatherIcon { get; set; }
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

    public class HourlyWeatherUnits
    {
        [JsonProperty("wind_speed_10m")]
        public string WindSpeed { get; set; }
    }

    public class HourlyWeather
    {
        [JsonProperty("time")]
        public List<string> Time { get; set; }

        [JsonProperty("temperature_2m")]
        public List<double> Temperature { get; set; }

        [JsonProperty("relative_humidity_2m")]
        public List<int> RelativeHumidity { get; set; }

        [JsonProperty("precipitation_probability")]
        public List<int> PrecipitationProbability { get; set; }

        [JsonProperty("weather_code")]
        public List<int> WeatherCode { get; set; }

        [JsonProperty("wind_speed_10m")]
        public List<double> WindSpeed { get; set; }

        [JsonProperty("wind_direction_10m")]
        public List<int> WindDirection { get; set; }

        [JsonProperty("is_day")]
        public List<bool> IsDay { get; set; }
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

    public class DailyWeatherUnits
    {
        [JsonProperty("wind_speed_10m_max")]
        public string WindSpeedMax { get; set; }
    }

    public class DailyWeather
    {
        [JsonProperty("time")]
        public List<string> Time { get; set; }

        [JsonProperty("weather_code")]
        public List<int> WeatherCode { get; set; }

        [JsonProperty("temperature_2m_max")]
        public List<double> TemperatureMax { get; set; }

        [JsonProperty("temperature_2m_min")]
        public List<double> TemperatureMin { get; set; }

        [JsonProperty("precipitation_probability_max")]
        public List<int> PrecipitationProbabilityMax { get; set; }

        [JsonProperty("wind_speed_10m_max")]
        public List<double> WindSpeedMax { get; set; }

        [JsonProperty("wind_direction_10m_dominant")]
        public List<int> WindDirectionDominant { get; set; }

        [JsonProperty("relative_humidity_2m_mean")]
        public List<int> RelativeHumidityMean { get; set; }
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
