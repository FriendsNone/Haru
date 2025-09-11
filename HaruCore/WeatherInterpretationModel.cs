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
            { 0, "clear" },
            { 1, "mostly clear" },
            { 2, "partly cloudy" },
            { 3, "overcast" },
            { 45, "fog" },
            { 48, "icy fog" },
            { 51, "light drizzle" },
            { 53, "drizzle" },
            { 55, "heavy drizzle" },
            { 56, "light icy drizzle" },
            { 57, "icy drizzle" },
            { 61, "light rain" },
            { 63, "rain" },
            { 65, "heavy rain" },
            { 66, "light icy rain" },
            { 67, "icy rain" },
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
            { 96, "thunderstorm + light hail" },
            { 99, "thunderstorm + hail" }
        };

        private static readonly Dictionary<int, string> WeatherIcons = new Dictionary<int, string>
        {
            { 0, "/Assets/WeatherIcons/clear-day.png" },
            { 1, "/Assets/WeatherIcons/cloudy.png" },
            { 2, "/Assets/WeatherIcons/overcast.png" },
            { 3, "/Assets/WeatherIcons/extreme.png" },
            { 45, "/Assets/WeatherIcons/fog.png" },
            { 48, "/Assets/WeatherIcons/overcast-fog.png" },
            { 51, "/Assets/WeatherIcons/drizzle.png" },
            { 53, "/Assets/WeatherIcons/overcast-drizzle.png" },
            { 55, "/Assets/WeatherIcons/extreme-drizzle.png" },
            { 56, "/Assets/WeatherIcons/sleet.png" },
            { 57, "/Assets/WeatherIcons/overcast-sleet.png" },
            { 61, "/Assets/WeatherIcons/rain.png" },
            { 63, "/Assets/WeatherIcons/overcast-rain.png" },
            { 65, "/Assets/WeatherIcons/extreme-rain.png" },
            { 66, "/Assets/WeatherIcons/sleet.png" },
            { 67, "/Assets/WeatherIcons/overcast-sleet.png" },
            { 71, "/Assets/WeatherIcons/snow.png" },
            { 73, "/Assets/WeatherIcons/overcast-snow.png" },
            { 75, "/Assets/WeatherIcons/extreme-snow.png" },
            { 77, "/Assets/WeatherIcons/snowflake.png" },
            { 80, "/Assets/WeatherIcons/rain.png" },
            { 81, "/Assets/WeatherIcons/overcast-rain.png" },
            { 82, "/Assets/WeatherIcons/extreme-rain.png" },
            { 85, "/Assets/WeatherIcons/snow.png" },
            { 86, "/Assets/WeatherIcons/overcast-snow.png" },
            { 95, "/Assets/WeatherIcons/thunderstorms-extreme.png" },
            { 96, "/Assets/WeatherIcons/thunderstorms-extreme-rain.png" },
            { 99, "/Assets/WeatherIcons/thunderstorms-extreme-rain.png" }
        };

        private static readonly Dictionary<int, string> WeatherIconsDay = new Dictionary<int, string>
        {
            { 0, "/Assets/WeatherIcons/clear-day.png" },
            { 1, "/Assets/WeatherIcons/cloudy.png" },
            { 2, "/Assets/WeatherIcons/overcast.png" },
            { 3, "/Assets/WeatherIcons/extreme.png" },
            { 45, "/Assets/WeatherIcons/fog.png" },
            { 48, "/Assets/WeatherIcons/overcast-fog.png" },
            { 51, "/Assets/WeatherIcons/drizzle.png" },
            { 53, "/Assets/WeatherIcons/overcast-drizzle.png" },
            { 55, "/Assets/WeatherIcons/extreme-drizzle.png" },
            { 56, "/Assets/WeatherIcons/sleet.png" },
            { 57, "/Assets/WeatherIcons/overcast-sleet.png" },
            { 61, "/Assets/WeatherIcons/rain.png" },
            { 63, "/Assets/WeatherIcons/overcast-rain.png" },
            { 65, "/Assets/WeatherIcons/extreme-rain.png" },
            { 66, "/Assets/WeatherIcons/sleet.png" },
            { 67, "/Assets/WeatherIcons/overcast-sleet.png" },
            { 71, "/Assets/WeatherIcons/snow.png" },
            { 73, "/Assets/WeatherIcons/overcast-snow.png" },
            { 75, "/Assets/WeatherIcons/extreme-snow.png" },
            { 77, "/Assets/WeatherIcons/snowflake.png" },
            { 80, "/Assets/WeatherIcons/rain.png" },
            { 81, "/Assets/WeatherIcons/overcast-rain.png" },
            { 82, "/Assets/WeatherIcons/extreme-rain.png" },
            { 85, "/Assets/WeatherIcons/snow.png" },
            { 86, "/Assets/WeatherIcons/overcast-snow.png" },
            { 95, "/Assets/WeatherIcons/thunderstorms-extreme.png" },
            { 96, "/Assets/WeatherIcons/thunderstorms-extreme-rain.png" },
            { 99, "/Assets/WeatherIcons/thunderstorms-extreme-rain.png" }
        };

        private static readonly Dictionary<int, string> WeatherIconsNight = new Dictionary<int, string>
        {
            { 0, "/Assets/WeatherIcons/clear-day.png" },
            { 1, "/Assets/WeatherIcons/cloudy.png" },
            { 2, "/Assets/WeatherIcons/overcast.png" },
            { 3, "/Assets/WeatherIcons/extreme.png" },
            { 45, "/Assets/WeatherIcons/fog.png" },
            { 48, "/Assets/WeatherIcons/overcast-fog.png" },
            { 51, "/Assets/WeatherIcons/drizzle.png" },
            { 53, "/Assets/WeatherIcons/overcast-drizzle.png" },
            { 55, "/Assets/WeatherIcons/extreme-drizzle.png" },
            { 56, "/Assets/WeatherIcons/sleet.png" },
            { 57, "/Assets/WeatherIcons/overcast-sleet.png" },
            { 61, "/Assets/WeatherIcons/rain.png" },
            { 63, "/Assets/WeatherIcons/overcast-rain.png" },
            { 65, "/Assets/WeatherIcons/extreme-rain.png" },
            { 66, "/Assets/WeatherIcons/sleet.png" },
            { 67, "/Assets/WeatherIcons/overcast-sleet.png" },
            { 71, "/Assets/WeatherIcons/snow.png" },
            { 73, "/Assets/WeatherIcons/overcast-snow.png" },
            { 75, "/Assets/WeatherIcons/extreme-snow.png" },
            { 77, "/Assets/WeatherIcons/snowflake.png" },
            { 80, "/Assets/WeatherIcons/rain.png" },
            { 81, "/Assets/WeatherIcons/overcast-rain.png" },
            { 82, "/Assets/WeatherIcons/extreme-rain.png" },
            { 85, "/Assets/WeatherIcons/snow.png" },
            { 86, "/Assets/WeatherIcons/overcast-snow.png" },
            { 95, "/Assets/WeatherIcons/thunderstorms-extreme.png" },
            { 96, "/Assets/WeatherIcons/thunderstorms-extreme-rain.png" },
            { 99, "/Assets/WeatherIcons/thunderstorms-extreme-rain.png" }
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
            if (weatherCode == 1) return isDay ? "mostly sunny" : "mostly clear";

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