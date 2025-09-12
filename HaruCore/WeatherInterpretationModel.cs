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
            { 1, "clear" },
            { 2, "partly cloudy" },
            { 3, "overcast" },
            { 45, "fog" },
            { 48, "fog" },
            { 51, "light drizzle" },
            { 53, "moderate drizzle" },
            { 55, "heavy drizzle" },
            { 56, "light freezing drizzle" },
            { 57, "dense freezing drizzle" },
            { 61, "light rain" },
            { 63, "moderate rain" },
            { 65, "heavy rain" },
            { 66, "light freezing rain" },
            { 67, "heavy freezing rain" },
            { 71, "light snow" },
            { 73, "moderate snow" },
            { 75, "heavy snow" },
            { 77, "snow grains" },
            { 80, "light rain showers" },
            { 81, "moderate rain showers" },
            { 82, "heavy rain showers" },
            { 85, "light snow showers" },
            { 86, "heavy snow showers" },
            { 95, "thunderstorm" },
            { 96, "light hail" },
            { 99, "heavy hail" }
        };

        private static readonly Dictionary<int, string> WeatherIcons = new Dictionary<int, string>
        {
            { 0, "/Assets/WeatherIcons/clear-day.png" },
            { 1, "/Assets/WeatherIcons/clear-day.png" },
            { 2, "/Assets/WeatherIcons/cloudy.png" },
            { 3, "/Assets/WeatherIcons/cloudy.png" },
            { 45, "/Assets/WeatherIcons/mist.png" },
            { 48, "/Assets/WeatherIcons/mist.png" },
            { 51, "/Assets/WeatherIcons/rain.png" },
            { 53, "/Assets/WeatherIcons/rain.png" },
            { 55, "/Assets/WeatherIcons/rain.png" },
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
            { 77, "/Assets/WeatherIcons/snow.png" },
            { 80, "/Assets/WeatherIcons/rain.png" },
            { 81, "/Assets/WeatherIcons/rain.png" },
            { 82, "/Assets/WeatherIcons/rain.png" },
            { 85, "/Assets/WeatherIcons/sleet.png" },
            { 86, "/Assets/WeatherIcons/sleet.png" },
            { 95, "/Assets/WeatherIcons/thunderstorms.png" },
            { 96, "/Assets/WeatherIcons/thunderstorms.png" },
            { 99, "/Assets/WeatherIcons/thunderstorms.png" }
        };

        private static readonly Dictionary<int, string> WeatherTileIcons = new Dictionary<int, string>
        {
            { 0, "/Assets/WeatherIcons/Tile/clear-day.png" },
            { 1, "/Assets/WeatherIcons/Tile/clear-day.png" },
            { 2, "/Assets/WeatherIcons/Tile/cloudy.png" },
            { 3, "/Assets/WeatherIcons/Tile/cloudy.png" },
            { 45, "/Assets/WeatherIcons/Tile/mist.png" },
            { 48, "/Assets/WeatherIcons/Tile/mist.png" },
            { 51, "/Assets/WeatherIcons/Tile/rain.png" },
            { 53, "/Assets/WeatherIcons/Tile/rain.png" },
            { 55, "/Assets/WeatherIcons/Tile/rain.png" },
            { 56, "/Assets/WeatherIcons/Tile/sleet.png" },
            { 57, "/Assets/WeatherIcons/Tile/sleet.png" },
            { 61, "/Assets/WeatherIcons/Tile/rain.png" },
            { 63, "/Assets/WeatherIcons/Tile/rain.png" },
            { 65, "/Assets/WeatherIcons/Tile/rain.png" },
            { 66, "/Assets/WeatherIcons/Tile/sleet.png" },
            { 67, "/Assets/WeatherIcons/Tile/sleet.png" },
            { 71, "/Assets/WeatherIcons/Tile/snow.png" },
            { 73, "/Assets/WeatherIcons/Tile/snow.png" },
            { 75, "/Assets/WeatherIcons/Tile/snow.png" },
            { 77, "/Assets/WeatherIcons/Tile/snow.png" },
            { 80, "/Assets/WeatherIcons/Tile/rain.png" },
            { 81, "/Assets/WeatherIcons/Tile/rain.png" },
            { 82, "/Assets/WeatherIcons/Tile/rain.png" },
            { 85, "/Assets/WeatherIcons/Tile/sleet.png" },
            { 86, "/Assets/WeatherIcons/Tile/sleet.png" },
            { 95, "/Assets/WeatherIcons/Tile/thunderstorms.png" },
            { 96, "/Assets/WeatherIcons/Tile/thunderstorms.png" },
            { 99, "/Assets/WeatherIcons/Tile/thunderstorms.png" }
        };

        public static string GetWeatherDescription(int weatherCode, bool isDay)
        {
            if (weatherCode == 0 || weatherCode == 1) return isDay ? "sunny" : "clear";

            string description;
            if (WeatherDescriptions.TryGetValue(weatherCode, out description))
                return description;

            return "unknown";
        }

        public static string GetWeatherIcon(int weatherCode, bool isDay)
        {
            if (weatherCode == 0 || weatherCode == 1) return isDay ? "/Assets/WeatherIcons/clear-day.png" : "/Assets/WeatherIcons/clear-night.png";

            string iconPath;
            if (WeatherIcons.TryGetValue(weatherCode, out iconPath))
                return iconPath;

            return "/Assets/WeatherIcons/not-available.png";
        }

        public static string GetWeatherTileIcon(int weatherCode, bool isDay)
        {
            if (weatherCode == 0 || weatherCode == 1) return isDay ? "/Assets/WeatherIcons/Tile/clear-day.png" : "/Assets/WeatherIcons/Tile/clear-night.png";

            string iconPath;
            if (WeatherTileIcons.TryGetValue(weatherCode, out iconPath))
                return iconPath;

            return "/Assets/WeatherIcons/Tile/not-available.png";
        }
    }
}