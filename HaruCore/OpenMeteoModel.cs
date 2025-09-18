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
        public int Temperature2m { get; set; }
        public double RelativeHumidity2m { get; set; }
        public int PrecipitationProbability { get; set; }
        public string WeatherIcon { get; set; }
        public string WeatherDescription { get; set; }
        public double WindSpeed10m { get; set; }
        public int WindDirection10m { get; set; }
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
                    Time = string.Format("{0} {1}", time.ToString("ddd", CultureInfo.CurrentCulture), time.ToString("t", CultureInfo.CurrentCulture)),
                    Temperature2m = (int)Math.Ceiling(Temperature2m.ElementAtOrDefault(i)),
                    RelativeHumidity2m = RelativeHumidity2m.ElementAtOrDefault(i),
                    PrecipitationProbability = PrecipitationProbability.ElementAtOrDefault(i),
                    WeatherIcon = WeatherInterpretationModel.GetWeatherIcon(WeatherCode.ElementAtOrDefault(i), IsDay.ElementAtOrDefault(i)),
                    WeatherDescription = WeatherInterpretationModel.GetWeatherDescription(WeatherCode.ElementAtOrDefault(i), IsDay.ElementAtOrDefault(i)),
                    WindSpeed10m = WindSpeed10m.ElementAtOrDefault(i),
                    WindDirection10m = WindDirection10m.ElementAtOrDefault(i),
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
        public int Temperature2mMax { get; set; }
        public int Temperature2mMin { get; set; }
        public double PrecipitationSum { get; set; }
        public int PrecipitationProbabilityMax { get; set; }
        public double WindSpeed10mMax { get; set; }
        public int WindDirection10mDominant { get; set; }
        public int PrecipitationProbabilityMean { get; set; }
        public int PrecipitationProbabilityMin { get; set; }
        public int RelativeHumidity2mMean { get; set; }
        public int RelativeHumidity2mMax { get; set; }
        public int RelativeHumidity2mMin { get; set; }
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

        [JsonProperty("precipitation_sum")]
        public List<double> PrecipitationSum { get; set; }

        [JsonProperty("precipitation_probability_max")]
        public List<int> PrecipitationProbabilityMax { get; set; }

        [JsonProperty("wind_speed_10m_max")]
        public List<double> WindSpeed10mMax { get; set; }

        [JsonProperty("wind_direction_10m_dominant")]
        public List<int> WindDirection10mDominant { get; set; }

        [JsonProperty("precipitation_probability_mean")]
        public List<int> PrecipitationProbabilityMean { get; set; }

        [JsonProperty("precipitation_probability_min")]
        public List<int> PrecipitationProbabilityMin { get; set; }

        [JsonProperty("relative_humidity_2m_mean")]
        public List<int> RelativeHumidity2mMean { get; set; }

        [JsonProperty("relative_humidity_2m_max")]
        public List<int> RelativeHumidity2mMax { get; set; }

        [JsonProperty("relative_humidity_2m_min")]
        public List<int> RelativeHumidity2mMin { get; set; }

        public List<DailyRecord> ToRecords()
        {
            var records = new List<DailyRecord>();
            for (int i = 0; i < Time.Count; i++)
            {
                records.Add(new DailyRecord
                {
                    Time = DateTime.Parse(Time[i]).ToString("ddd M/dd", CultureInfo.CurrentCulture),
                    WeatherIcon = WeatherInterpretationModel.GetWeatherIcon(WeatherCode.ElementAtOrDefault(i), true),
                    WeatherDescription = WeatherInterpretationModel.GetWeatherDescription(WeatherCode.ElementAtOrDefault(i), true),
                    Temperature2mMax = (int)Math.Ceiling(Temperature2mMax.ElementAtOrDefault(i)),
                    Temperature2mMin = (int)Math.Ceiling(Temperature2mMin.ElementAtOrDefault(i)),
                    PrecipitationSum = PrecipitationSum.ElementAtOrDefault(i),
                    PrecipitationProbabilityMax = PrecipitationProbabilityMax.ElementAtOrDefault(i),
                    WindSpeed10mMax = WindSpeed10mMax.ElementAtOrDefault(i),
                    WindDirection10mDominant = WindDirection10mDominant.ElementAtOrDefault(i),
                });
            }
            return records;
        }
    }
}
