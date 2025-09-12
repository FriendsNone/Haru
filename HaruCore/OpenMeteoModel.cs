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

    public class HourlyRecord
    {
        public string Time { get; set; }
        public int Temperature2m { get; set; }
        public double RelativeHumidity2m { get; set; }
        public double DewPoint2m { get; set; }
        public int ApparentTemperature { get; set; }
        public int PrecipitationProbability { get; set; }
        public double Precipitation { get; set; }
        public string WeatherIcon { get; set; }
        public string WeatherDescription { get; set; }
        public double PressureMsl { get; set; }
        public double SurfacePressure { get; set; }
        public int CloudCover { get; set; }
        public int CloudCoverLow { get; set; }
        public int CloudCoverMid { get; set; }
        public int CloudCoverHigh { get; set; }
        public double Visibility { get; set; }
        public double Evapotranspiration { get; set; }
        public double Et0FaoEvapotranspiration { get; set; }
        public double VapourPressureDeficit { get; set; }
        public double WindSpeed10m { get; set; }
        public double WindSpeed80m { get; set; }
        public double WindSpeed120m { get; set; }
        public double WindSpeed180m { get; set; }
        public int WindDirection10m { get; set; }
        public int WindDirection80m { get; set; }
        public int WindDirection120m { get; set; }
        public int WindDirection180m { get; set; }
        public double WindGusts10m { get; set; }
    }

    public class HourlyWeather
    {
        [JsonProperty("time")]
        public List<string> Time { get; set; }

        [JsonProperty("temperature_2m")]
        public List<double> Temperature2m { get; set; }

        [JsonProperty("relative_humidity_2m")]
        public List<double> RelativeHumidity2m { get; set; }

        [JsonProperty("dew_point_2m")]
        public List<double> DewPoint2m { get; set; }

        [JsonProperty("apparent_temperature")]
        public List<double> ApparentTemperature { get; set; }

        [JsonProperty("precipitation_probability")]
        public List<int> PrecipitationProbability { get; set; }

        [JsonProperty("precipitation")]
        public List<double> Precipitation { get; set; }

        [JsonProperty("rain")]
        public List<double> Rain { get; set; }

        [JsonProperty("showers")]
        public List<double> Showers { get; set; }

        [JsonProperty("snowfall")]
        public List<double> Snowfall { get; set; }

        [JsonProperty("snow_depth")]
        public List<double> SnowDepth { get; set; }

        [JsonProperty("weather_code")]
        public List<int> WeatherCode { get; set; }

        [JsonProperty("pressure_msl")]
        public List<double> PressureMsl { get; set; }

        [JsonProperty("surface_pressure")]
        public List<double> SurfacePressure { get; set; }

        [JsonProperty("cloud_cover")]
        public List<int> CloudCover { get; set; }

        [JsonProperty("cloud_cover_low")]
        public List<int> CloudCoverLow { get; set; }

        [JsonProperty("cloud_cover_mid")]
        public List<int> CloudCoverMid { get; set; }

        [JsonProperty("cloud_cover_high")]
        public List<int> CloudCoverHigh { get; set; }

        [JsonProperty("visibility")]
        public List<double> Visibility { get; set; }

        [JsonProperty("evapotranspiration")]
        public List<double> Evapotranspiration { get; set; }

        [JsonProperty("et0_fao_evapotranspiration")]
        public List<double> Et0FaoEvapotranspiration { get; set; }

        [JsonProperty("vapour_pressure_deficit")]
        public List<double> VapourPressureDeficit { get; set; }

        [JsonProperty("wind_speed_10m")]
        public List<double> WindSpeed10m { get; set; }

        [JsonProperty("wind_speed_80m")]
        public List<double> WindSpeed80m { get; set; }

        [JsonProperty("wind_speed_120m")]
        public List<double> WindSpeed120m { get; set; }

        [JsonProperty("wind_speed_180m")]
        public List<double> WindSpeed180m { get; set; }

        [JsonProperty("wind_direction_10m")]
        public List<int> WindDirection10m { get; set; }

        [JsonProperty("wind_direction_80m")]
        public List<int> WindDirection80m { get; set; }

        [JsonProperty("wind_direction_120m")]
        public List<int> WindDirection120m { get; set; }

        [JsonProperty("wind_direction_180m")]
        public List<int> WindDirection180m { get; set; }

        [JsonProperty("wind_gusts_10m")]
        public List<double> WindGusts10m { get; set; }

        [JsonProperty("temperature_80m")]
        public List<double> Temperature80m { get; set; }

        [JsonProperty("temperature_120m")]
        public List<double> Temperature120m { get; set; }

        [JsonProperty("temperature_180m")]
        public List<double> Temperature180m { get; set; }

        [JsonProperty("soil_temperature_0cm")]
        public List<double> SoilTemperature0cm { get; set; }

        [JsonProperty("soil_temperature_6cm")]
        public List<double> SoilTemperature6cm { get; set; }

        [JsonProperty("soil_temperature_18cm")]
        public List<double> SoilTemperature18cm { get; set; }

        [JsonProperty("soil_temperature_54cm")]
        public List<double> SoilTemperature54cm { get; set; }

        [JsonProperty("soil_moisture_0_to_1cm")]
        public List<double> SoilMoisture0to1cm { get; set; }

        [JsonProperty("soil_moisture_1_to_3cm")]
        public List<double> SoilMoisture1to3cm { get; set; }

        [JsonProperty("soil_moisture_3_to_9cm")]
        public List<double> SoilMoisture3to9cm { get; set; }

        [JsonProperty("soil_moisture_9_to_27cm")]
        public List<double> SoilMoisture9to27cm { get; set; }

        [JsonProperty("soil_moisture_27_to_81cm")]
        public List<double> SoilMoisture27to81cm { get; set; }

        [JsonProperty("is_day")]
        public List<bool> IsDay { get; set; }

        public List<HourlyRecord> ToRecords()
        {
            var records = new List<HourlyRecord>();
            for (int i = 0; i < Time.Count; i++)
            {
                records.Add(new HourlyRecord
                {
                    Time = DateTime.Parse(Time[i]).ToString("t", CultureInfo.CurrentCulture),
                    Temperature2m = (int)Math.Ceiling(Temperature2m.ElementAtOrDefault(i)),
                    RelativeHumidity2m = RelativeHumidity2m.ElementAtOrDefault(i),
                    DewPoint2m = DewPoint2m.ElementAtOrDefault(i),
                    ApparentTemperature = (int)Math.Ceiling(ApparentTemperature.ElementAtOrDefault(i)),
                    PrecipitationProbability = PrecipitationProbability.ElementAtOrDefault(i),
                    Precipitation = Precipitation.ElementAtOrDefault(i),
                    WeatherIcon = WeatherInterpretationModel.GetWeatherIcon(WeatherCode.ElementAtOrDefault(i), IsDay.ElementAtOrDefault(i)),
                    WeatherDescription = WeatherInterpretationModel.GetWeatherDescription(WeatherCode.ElementAtOrDefault(i), IsDay.ElementAtOrDefault(i)),
                    PressureMsl = PressureMsl.ElementAtOrDefault(i),
                    SurfacePressure = SurfacePressure.ElementAtOrDefault(i),
                    CloudCover = CloudCover.ElementAtOrDefault(i),
                    CloudCoverLow = CloudCoverLow.ElementAtOrDefault(i),
                    CloudCoverMid = CloudCoverMid.ElementAtOrDefault(i),
                    CloudCoverHigh = CloudCoverHigh.ElementAtOrDefault(i),
                    Visibility = Visibility.ElementAtOrDefault(i),
                    Evapotranspiration = Evapotranspiration.ElementAtOrDefault(i),
                    Et0FaoEvapotranspiration = Et0FaoEvapotranspiration.ElementAtOrDefault(i),
                    VapourPressureDeficit = VapourPressureDeficit.ElementAtOrDefault(i),
                    WindSpeed10m = WindSpeed10m.ElementAtOrDefault(i),
                    WindSpeed80m = WindSpeed80m.ElementAtOrDefault(i),
                    WindSpeed120m = WindSpeed120m.ElementAtOrDefault(i),
                    WindSpeed180m = WindSpeed180m.ElementAtOrDefault(i),
                    WindDirection10m = WindDirection10m.ElementAtOrDefault(i),
                    WindDirection80m = WindDirection80m.ElementAtOrDefault(i),
                    WindDirection120m = WindDirection120m.ElementAtOrDefault(i),
                    WindDirection180m = WindDirection180m.ElementAtOrDefault(i),
                    WindGusts10m = WindGusts10m.ElementAtOrDefault(i),
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
        public int ApparentTemperatureMax { get; set; }
        public int ApparentTemperatureMin { get; set; }
        public DateTime Sunrise { get; set; }
        public DateTime Sunset { get; set; }
        public double DaylightDuration { get; set; }
        public double SunshineDuration { get; set; }
        public double UvIndexMax { get; set; }
        public double UvIndexClearSkyMax { get; set; }
        public double RainSum { get; set; }
        public double ShowersSum { get; set; }
        public double SnowfallSum { get; set; }
        public double PrecipitationSum { get; set; }
        public double PrecipitationHours { get; set; }
        public int PrecipitationProbabilityMax { get; set; }
        public double WindSpeed10mMax { get; set; }
        public double WindGusts10mMax { get; set; }
        public int WindDirection10mDominant { get; set; }
        public double ShortwaveRadiationSum { get; set; }
        public double Et0FaoEvapotranspiration { get; set; }
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

        [JsonProperty("apparent_temperature_max")]
        public List<double> ApparentTemperatureMax { get; set; }

        [JsonProperty("apparent_temperature_min")]
        public List<double> ApparentTemperatureMin { get; set; }

        [JsonProperty("sunrise")]
        public List<string> Sunrise { get; set; }

        [JsonProperty("sunset")]
        public List<string> Sunset { get; set; }

        [JsonProperty("daylight_duration")]
        public List<double> DaylightDuration { get; set; }

        [JsonProperty("sunshine_duration")]
        public List<double> SunshineDuration { get; set; }

        [JsonProperty("uv_index_max")]
        public List<double> UvIndexMax { get; set; }

        [JsonProperty("uv_index_clear_sky_max")]
        public List<double> UvIndexClearSkyMax { get; set; }

        [JsonProperty("rain_sum")]
        public List<double> RainSum { get; set; }

        [JsonProperty("showers_sum")]
        public List<double> ShowersSum { get; set; }

        [JsonProperty("snowfall_sum")]
        public List<double> SnowfallSum { get; set; }

        [JsonProperty("precipitation_sum")]
        public List<double> PrecipitationSum { get; set; }

        [JsonProperty("precipitation_hours")]
        public List<double> PrecipitationHours { get; set; }

        [JsonProperty("precipitation_probability_max")]
        public List<int> PrecipitationProbabilityMax { get; set; }

        [JsonProperty("wind_speed_10m_max")]
        public List<double> WindSpeed10mMax { get; set; }

        [JsonProperty("wind_gusts_10m_max")]
        public List<double> WindGusts10mMax { get; set; }

        [JsonProperty("wind_direction_10m_dominant")]
        public List<int> WindDirection10mDominant { get; set; }

        [JsonProperty("shortwave_radiation_sum")]
        public List<double> ShortwaveRadiationSum { get; set; }

        [JsonProperty("et0_fao_evapotranspiration")]
        public List<double> Et0FaoEvapotranspiration { get; set; }

        public List<DailyRecord> ToRecords()
        {
            var records = new List<DailyRecord>();
            for (int i = 0; i < Time.Count; i++)
            {
                records.Add(new DailyRecord
                {
                    Time = DateTime.Parse(Time[i]).ToString("d dddd", CultureInfo.CurrentCulture),
                    WeatherIcon = WeatherInterpretationModel.GetWeatherIcon(WeatherCode.ElementAtOrDefault(i), true),
                    WeatherDescription = WeatherInterpretationModel.GetWeatherDescription(WeatherCode.ElementAtOrDefault(i), true),
                    Temperature2mMax = (int)Math.Ceiling(Temperature2mMax.ElementAtOrDefault(i)),
                    Temperature2mMin = (int)Math.Ceiling(Temperature2mMin.ElementAtOrDefault(i)),
                    ApparentTemperatureMax = (int)Math.Ceiling(ApparentTemperatureMax.ElementAtOrDefault(i)),
                    ApparentTemperatureMin = (int)Math.Ceiling(ApparentTemperatureMin.ElementAtOrDefault(i)),
                    Sunrise = DateTime.Parse(Sunrise.ElementAtOrDefault(i)),
                    Sunset = DateTime.Parse(Sunset.ElementAtOrDefault(i)),
                    DaylightDuration = DaylightDuration.ElementAtOrDefault(i),
                    SunshineDuration = SunshineDuration.ElementAtOrDefault(i),
                    UvIndexMax = UvIndexMax.ElementAtOrDefault(i),
                    UvIndexClearSkyMax = UvIndexClearSkyMax.ElementAtOrDefault(i),
                    RainSum = RainSum.ElementAtOrDefault(i),
                    ShowersSum = ShowersSum.ElementAtOrDefault(i),
                    SnowfallSum = SnowfallSum.ElementAtOrDefault(i),
                    PrecipitationSum = PrecipitationSum.ElementAtOrDefault(i),
                    PrecipitationHours = PrecipitationHours.ElementAtOrDefault(i),
                    PrecipitationProbabilityMax = PrecipitationProbabilityMax.ElementAtOrDefault(i),
                    WindSpeed10mMax = WindSpeed10mMax.ElementAtOrDefault(i),
                    WindGusts10mMax = WindGusts10mMax.ElementAtOrDefault(i),
                    WindDirection10mDominant = WindDirection10mDominant.ElementAtOrDefault(i),
                    ShortwaveRadiationSum = ShortwaveRadiationSum.ElementAtOrDefault(i),
                    Et0FaoEvapotranspiration = Et0FaoEvapotranspiration.ElementAtOrDefault(i)
                });
            }
            return records;
        }
    }
}
