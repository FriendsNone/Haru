using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HaruCore
{
    public class WeatherInterpretationModel
    {
        private static readonly Dictionary<int, string> WeatherDescriptions = new Dictionary<int, string>
        {
            { 0, "clear sky" },
            { 1, "mainly clear" },
            { 2, "partly cloudy" },
            { 3, "cloudy" },
            { 45, "foggy" },
            { 48, "rime fog" },
            { 51, "light drizzle" },
            { 53, "drizzle" },
            { 55, "heavy drizzle" },
            { 56, "light freezing drizzle" },
            { 57, "freezing drizzle" },
            { 61, "light rain" },
            { 63, "rain" },
            { 65, "heavy rain" },
            { 66, "light freezing rain" },
            { 67, "freezing rain" },
            { 71, "light snow" },
            { 73, "snow" },
            { 75, "heavy snow" },
            { 77, "snow grains" },
            { 80, "light showers" },
            { 81, "showers" },
            { 82, "heavy showers" },
            { 85, "light snow showers" },
            { 86, "snow showers" },
            { 95, "thunderstorm" },
            { 96, "light thunderstorms with hail" },
            { 99, "thunderstorm with hail" }
        };

        private static readonly Dictionary<int, string> WeatherIcons = new Dictionary<int, string>
        {
            { 0, "/Assets/WeatherIcons/day/clear-day.png" },
            { 1, "/Assets/WeatherIcons/cloudy.png" },
            { 2, "/Assets/WeatherIcons/cloudy.png" },
            { 3, "/Assets/WeatherIcons/overcast.png" },
            { 45, "/Assets/WeatherIcons/fog.png" },
            { 48, "/Assets/WeatherIcons/fog.png" },
            { 51, "/Assets/WeatherIcons/drizzle.png" },
            { 53, "/Assets/WeatherIcons/drizzle.png" },
            { 55, "/Assets/WeatherIcons/drizzle.png" },
            { 56, "/Assets/WeatherIcons/sleet.png" },
            { 57, "/Assets/WeatherIcons/sleet.png" },
            { 61, "/Assets/WeatherIcons/rain.png" },
            { 63, "/Assets/WeatherIcons/rain.png" },
            { 65, "/Assets/WeatherIcons/rain.png" },
            { 66, "/Assets/WeatherIcons/sleet.png" },
            { 67, "/Assets/WeatherIcons/sleet.png" },
            { 71, "/Assets/WeatherIcons/snow.png" },
            { 73, "/Assets/WeatherIcons/snow.png" },
            { 75, "/Assets/WeatherIcons/snow.png" },
            { 77, "/Assets/WeatherIcons/snowflake.png" },
            { 80, "/Assets/WeatherIcons/rain.png" },
            { 81, "/Assets/WeatherIcons/rain.png" },
            { 82, "/Assets/WeatherIcons/rain.png" },
            { 85, "/Assets/WeatherIcons/snow.png" },
            { 86, "/Assets/WeatherIcons/snow.png" },
            { 95, "/Assets/WeatherIcons/thunderstorms.png" },
            { 96, "/Assets/WeatherIcons/thunderstorms-rain.png" },
            { 99, "/Assets/WeatherIcons/thunderstorms.png" }
        };        

        private static readonly Dictionary<int, string> WeatherIconsDay = new Dictionary<int, string>
        {
            { 0, "/Assets/WeatherIcons/day/clear-day.png" },
            { 1, "/Assets/WeatherIcons/day/partly-cloudy-day.png" },
            { 2, "/Assets/WeatherIcons/day/partly-cloudy-day.png" },
            { 3, "/Assets/WeatherIcons/day/overcast-day.png" },
            { 45, "/Assets/WeatherIcons/day/fog-day.png" },
            { 48, "/Assets/WeatherIcons/day/fog-day.png" },
            { 51, "/Assets/WeatherIcons/day/partly-cloudy-day-drizzle.png" },
            { 53, "/Assets/WeatherIcons/day/partly-cloudy-day-drizzle.png" },
            { 55, "/Assets/WeatherIcons/day/partly-cloudy-day-drizzle.png" },
            { 56, "/Assets/WeatherIcons/sleet.png" },
            { 57, "/Assets/WeatherIcons/sleet.png" },
            { 61, "/Assets/WeatherIcons/day/partly-cloudy-day-rain.png" },
            { 63, "/Assets/WeatherIcons/rain.png" },
            { 65, "/Assets/WeatherIcons/rain.png" },
            { 66, "/Assets/WeatherIcons/sleet.png" },
            { 67, "/Assets/WeatherIcons/sleet.png" },
            { 71, "/Assets/WeatherIcons/day/partly-cloudy-day-snow.png" },
            { 73, "/Assets/WeatherIcons/snow.png" },
            { 75, "/Assets/WeatherIcons/snow.png" },
            { 77, "/Assets/WeatherIcons/snowflake.png" },
            { 80, "/Assets/WeatherIcons/day/partly-cloudy-day-rain.png" },
            { 81, "/Assets/WeatherIcons/rain.png" },
            { 82, "/Assets/WeatherIcons/rain.png" },
            { 85, "/Assets/WeatherIcons/day/partly-cloudy-day-snow.png" },
            { 86, "/Assets/WeatherIcons/snow.png" },
            { 95, "/Assets/WeatherIcons/day/thunderstorms-day.png" },
            { 96, "/Assets/WeatherIcons/day/thunderstorms-day-rain.png" },
            { 99, "/Assets/WeatherIcons/day/thunderstorms-day.png" }
        };

        private static readonly Dictionary<int, string> WeatherIconsNight = new Dictionary<int, string>
        {
            { 0, "/Assets/WeatherIcons/night/clear-night.png" },
            { 1, "/Assets/WeatherIcons/night/partly-cloudy-night.png" },
            { 2, "/Assets/WeatherIcons/night/partly-cloudy-night.png" },
            { 3, "/Assets/WeatherIcons/night/overcast-night.png" },
            { 45, "/Assets/WeatherIcons/night/fog-night.png" },
            { 48, "/Assets/WeatherIcons/night/fog-night.png" },
            { 51, "/Assets/WeatherIcons/night/partly-cloudy-night-drizzle.png" },
            { 53, "/Assets/WeatherIcons/night/partly-cloudy-night-drizzle.png" },
            { 55, "/Assets/WeatherIcons/night/partly-cloudy-night-drizzle.png" },
            { 56, "/Assets/WeatherIcons/sleet.png" },
            { 57, "/Assets/WeatherIcons/sleet.png" },
            { 61, "/Assets/WeatherIcons/night/partly-cloudy-night-rain.png" },
            { 63, "/Assets/WeatherIcons/rain.png" },
            { 65, "/Assets/WeatherIcons/rain.png" },
            { 66, "/Assets/WeatherIcons/sleet.png" },
            { 67, "/Assets/WeatherIcons/sleet.png" },
            { 71, "/Assets/WeatherIcons/night/partly-cloudy-night-snow.png" },
            { 73, "/Assets/WeatherIcons/snow.png" },
            { 75, "/Assets/WeatherIcons/snow.png" },
            { 77, "/Assets/WeatherIcons/snowflake.png" },
            { 80, "/Assets/WeatherIcons/night/partly-cloudy-night-rain.png" },
            { 81, "/Assets/WeatherIcons/rain.png" },
            { 82, "/Assets/WeatherIcons/rain.png" },
            { 85, "/Assets/WeatherIcons/night/partly-cloudy-night-snow.png" },
            { 86, "/Assets/WeatherIcons/snow.png" },
            { 95, "/Assets/WeatherIcons/night/thunderstorms-night.png" },
            { 96, "/Assets/WeatherIcons/night/thunderstorms-night-rain.png" },
            { 99, "/Assets/WeatherIcons/night/thunderstorms-night.png" }
        };

        public static string GetWeatherDescription(int weatherCode)
        {
            string description;
            if (WeatherDescriptions.TryGetValue(weatherCode, out description))
                return description;

            return "unknown";
        }

        public static string GetWeatherDescription(int weatherCode, bool isDay)
        {
            if (weatherCode == 0) return isDay ? "sunny" : "clear";
            if (weatherCode == 1) return isDay ? "mainly sunny" : "mainly clear";

            string description;
            if (WeatherDescriptions.TryGetValue(weatherCode, out description))
                return description;

            return "unknown";
        }

        public static string GetWeatherIcon(int weatherCode)
        {
            Dictionary<int, string> icons = WeatherIcons;
            string iconPath;
            if (icons.TryGetValue(weatherCode, out iconPath))
                return iconPath;

            return "/Assets/WeatherIcons/not-available.png";
        }

        public static string GetWeatherIcon(int weatherCode, bool isDay)
        {
            Dictionary<int, string> icons = isDay ? WeatherIconsDay : WeatherIconsNight;
            string iconPath;
            if (icons.TryGetValue(weatherCode, out iconPath))
                return iconPath;

            return "/Assets/WeatherIcons/not-available.png";
        }
    }
}