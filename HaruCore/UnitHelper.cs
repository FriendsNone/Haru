using System;
using System.Collections.Generic;
using System.Globalization;

namespace HaruCore
{
    public class UnitHelper
    {
        private const string IconBase = "/Assets/WeatherIcons/";
        private const string TileBase = "/Assets/WeatherIcons/Tile/";

        private static readonly string[] Directions = { "north", "northeast", "east", "southeast", "south", "southwest", "west", "northwest" };
        private static readonly string[] DirectionsShort = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };

        private static readonly Dictionary<int, string> WeatherDescriptions = new Dictionary<int, string>
        {
            { 0, "clear" }, { 1, "clear" }, { 2, "partly cloudy" }, { 3, "overcast" },
            { 45, "fog" }, { 48, "fog" }, { 51, "light drizzle" }, { 53, "moderate drizzle" },
            { 55, "heavy drizzle" }, { 56, "light freezing drizzle" }, { 57, "dense freezing drizzle" },
            { 61, "light rain" }, { 63, "moderate rain" }, { 65, "heavy rain" },
            { 66, "light freezing rain" }, { 67, "heavy freezing rain" },
            { 71, "light snow" }, { 73, "moderate snow" }, { 75, "heavy snow" }, { 77, "snow grains" },
            { 80, "light rain showers" }, { 81, "moderate rain showers" }, { 82, "heavy rain showers" },
            { 85, "light snow showers" }, { 86, "heavy snow showers" },
            { 95, "thunderstorm" }, { 96, "light hail" }, { 99, "heavy hail" }
        };

        private static readonly Dictionary<int, string> IconMap = new Dictionary<int, string>
        {
            { 0, "clear-day" }, { 1, "clear-day" }, { 2, "cloudy" }, { 3, "cloudy" },
            { 45, "mist" }, { 48, "mist" },
            { 51, "rain" }, { 53, "rain" }, { 55, "rain" }, { 61, "rain" }, { 63, "rain" }, { 65, "rain" },
            { 80, "rain" }, { 81, "rain" }, { 82, "rain" },
            { 56, "sleet" }, { 57, "sleet" }, { 66, "sleet" }, { 67, "sleet" }, { 85, "sleet" }, { 86, "sleet" },
            { 71, "snow" }, { 73, "snow" }, { 75, "snow" }, { 77, "snow" },
            { 95, "thunderstorms" }, { 96, "thunderstorms" }, { 99, "thunderstorms" }
        };

        public static string InterpretDirection(double azimuth, bool shorthand)
        {
            azimuth = (azimuth % 360 + 360) % 360;
            int index = (int)Math.Round(azimuth / 45.0) % 8;
            return shorthand ? DirectionsShort[index] : Directions[index];
        }

        public static string InterpretTimeDifference(string dateTime)
        {
            var dt = DateTime.Parse(dateTime, null, DateTimeStyles.RoundtripKind);
            var diff = DateTime.Now - dt;

            if (diff.TotalSeconds < 60) return "now";
            if (diff.TotalMinutes < 60) return string.Format("{0} minute{1} ago", (int)diff.TotalMinutes, diff.TotalMinutes >= 2 ? "s" : "");
            if (diff.TotalHours < 24) return string.Format("{0} hour{1} ago", (int)diff.TotalHours, diff.TotalHours >= 2 ? "s" : "");
            return dt.ToString("t", CultureInfo.CurrentCulture);
        }

        public static string GetWeatherDescription(int weatherCode, bool isDay)
        {
            if (weatherCode <= 1) return isDay ? "sunny" : "clear";
            string desc;
            return WeatherDescriptions.TryGetValue(weatherCode, out desc) ? desc : "unknown";
        }

        public static string GetWeatherIcon(int weatherCode, bool isDay)
        {
            if (weatherCode <= 1) return IconBase + (isDay ? "clear-day.png" : "clear-night.png");
            string icon;
            return IconMap.TryGetValue(weatherCode, out icon) ? IconBase + icon + ".png" : IconBase + "not-available.png";
        }

        public static string GetWeatherTileIcon(int weatherCode, bool isDay)
        {
            if (weatherCode <= 1) return TileBase + (isDay ? "clear-day.png" : "clear-night.png");
            string icon;
            return IconMap.TryGetValue(weatherCode, out icon) ? TileBase + icon + ".png" : TileBase + "not-available.png";
        }
    }
}