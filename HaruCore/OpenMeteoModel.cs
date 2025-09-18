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
        [JsonProperty("current")]
        public CurrentWeather Current { get; set; }

        [JsonProperty("hourly")]
        public HourlyWeather Hourly { get; set; }

        [JsonProperty("daily")]
        public DailyWeather Daily { get; set; }
    }

    public class CurrentWeather
    {
        [JsonProperty("time")]
        public string Time { get; set; }

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

        [JsonProperty("weather_code")]
        public int WeatherCode { get; set; }

        [JsonProperty("pressure_msl")]
        public double Pressure { get; set; }

        [JsonProperty("wind_speed_10m")]
        public double WindSpeed { get; set; }

        [JsonProperty("wind_direction_10m")]
        public double WindDirection { get; set; }
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

    public class HourlyWeather
    {
        [JsonProperty("time")]
        public List<string> Time { get; set; }

        [JsonProperty("temperature_2m")]
        public List<double> Temperature2m { get; set; }

        [JsonProperty("relative_humidity_2m")]
        public List<double> RelativeHumidity2m { get; set; }

        [JsonProperty("precipitation_probability")]
        public List<int> PrecipitationProbability { get; set; }

        [JsonProperty("weather_code")]
        public List<int> WeatherCode { get; set; }

        [JsonProperty("wind_speed_10m")]
        public List<double> WindSpeed10m { get; set; }

        [JsonProperty("wind_direction_10m")]
        public List<int> WindDirection10m { get; set; }

        [JsonProperty("is_day")]
        public List<bool> IsDay { get; set; }

        public List<HourlyRecord> ToRecords()
        {
            var records = new List<HourlyRecord>();
            for (int i = 0; i < Time.Count; i++)
            {
                DateTime time = DateTime.Parse(Time[i], null, DateTimeStyles.RoundtripKind);

                records.Add(new HourlyRecord
                {
                    Time = string.Format("{0} {1}", time.ToString("ddd", CultureInfo.CurrentCulture), time.ToString("t", CultureInfo.CurrentCulture)).ToUpper(),
                    WeatherIcon = WeatherInterpretationModel.GetWeatherIcon(WeatherCode.ElementAtOrDefault(i), IsDay.ElementAtOrDefault(i)),
                    WeatherDescription = WeatherInterpretationModel.GetWeatherDescription(WeatherCode.ElementAtOrDefault(i), IsDay.ElementAtOrDefault(i)),
                    Temperature = string.Format("{0}°", Math.Ceiling(Temperature2m.ElementAtOrDefault(i))),
                    Humidity = string.Format("{0}%", RelativeHumidity2m.ElementAtOrDefault(i)),
                    Precipitation = string.Format("{0}%", PrecipitationProbability.ElementAtOrDefault(i)),
                    Wind = string.Format("{0} {1}", WindSpeed10m.ElementAtOrDefault(i), UnitModel.InterpretDirection(WindDirection10m.ElementAtOrDefault(i), true)),
                });
            }
            return records;
        }
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

    public class DailyWeather
    {
        [JsonProperty("time")]
        public List<string> Time { get; set; }

        [JsonProperty("weather_code")]
        public List<int> WeatherCode { get; set; }

        [JsonProperty("temperature_2m_max")]
        public List<double> Temperature2mMax { get; set; }

        [JsonProperty("temperature_2m_min")]
        public List<double> Temperature2mMin { get; set; }

        [JsonProperty("precipitation_probability_max")]
        public List<int> PrecipitationProbabilityMax { get; set; }

        [JsonProperty("wind_speed_10m_max")]
        public List<double> WindSpeed10mMax { get; set; }

        [JsonProperty("wind_direction_10m_dominant")]
        public List<double> WindDirection10mDominant { get; set; }

        [JsonProperty("relative_humidity_2m_mean")]
        public List<int> RelativeHumidity2mMean { get; set; }

        public List<DailyRecord> ToRecords()
        {
            var records = new List<DailyRecord>();
            for (int i = 0; i < Time.Count; i++)
            {
                records.Add(new DailyRecord
                {
                    Time = DateTime.Parse(Time[i]).ToString("ddd M/dd", CultureInfo.CurrentCulture).ToUpper(),
                    WeatherIcon = WeatherInterpretationModel.GetWeatherIcon(WeatherCode.ElementAtOrDefault(i), true),
                    WeatherDescription = WeatherInterpretationModel.GetWeatherDescription(WeatherCode.ElementAtOrDefault(i), true),
                    Temperature = string.Format("{0}°/{1}°", Math.Ceiling(Temperature2mMax.ElementAtOrDefault(i)), Math.Ceiling(Temperature2mMin.ElementAtOrDefault(i))),
                    Humidity = string.Format("{0}%", RelativeHumidity2mMean.ElementAtOrDefault(i)),
                    Precipitation = string.Format("{0}%", PrecipitationProbabilityMax.ElementAtOrDefault(i)),
                    Wind = string.Format("{0} {1}", WindSpeed10mMax.ElementAtOrDefault(i), UnitModel.InterpretDirection(WindDirection10mDominant.ElementAtOrDefault(i), true)),
                });
            }
            return records;
        }
    }
}
