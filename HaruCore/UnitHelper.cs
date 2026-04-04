using HaruCore.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace HaruCore
{
    public class UnitHelper
    {
        private const string IconBase = "/Assets/WeatherIcons/";
        private const string TileBase = "/Assets/WeatherIcons/Tile/";

        private static string[] GetDirections()
        {
            return new[]
            {
                CoreResources.DirectionNorth,
                CoreResources.DirectionNortheast,
                CoreResources.DirectionEast,
                CoreResources.DirectionSoutheast,
                CoreResources.DirectionSouth,
                CoreResources.DirectionSouthwest,
                CoreResources.DirectionWest,
                CoreResources.DirectionNorthwest
            };
        }

        private static readonly string[] DirectionsShort = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };

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
            return shorthand ? DirectionsShort[index] : GetDirections()[index];
        }

        public static string InterpretTimeDifference(string dateTime)
        {
            var dt = DateTime.Parse(dateTime, null, DateTimeStyles.RoundtripKind);
            var diff = DateTime.Now - dt;

            if (diff.TotalSeconds < 60) return CoreResources.TimeNow;

            if (diff.TotalMinutes < 60)
            {
                int mins = (int)diff.TotalMinutes;
                return string.Format(mins == 1
                    ? CoreResources.TimeMinuteAgo
                    : CoreResources.TimeMinutesAgo, mins);
            }

            if (diff.TotalHours < 24)
            {
                int hours = (int)diff.TotalHours;
                return string.Format(hours == 1
                    ? CoreResources.TimeHourAgo
                    : CoreResources.TimeHoursAgo, hours);
            }

            return dt.ToString("t", CultureInfo.CurrentCulture);
        }

        public static string GetWeatherDescription(int weatherCode, bool isDay)
        {
            if (weatherCode <= 1) return isDay ? CoreResources.WeatherSunny : CoreResources.WeatherClear;

            switch (weatherCode)
            {
                case 2: return CoreResources.WeatherPartlyCloudy;
                case 3: return CoreResources.WeatherOvercast;
                case 45:
                case 48: return CoreResources.WeatherFog;
                case 51: return CoreResources.WeatherLightDrizzle;
                case 53: return CoreResources.WeatherModerateDrizzle;
                case 55: return CoreResources.WeatherHeavyDrizzle;
                case 56: return CoreResources.WeatherLightFreezingDrizzle;
                case 57: return CoreResources.WeatherDenseFreezingDrizzle;
                case 61: return CoreResources.WeatherLightRain;
                case 63: return CoreResources.WeatherModerateRain;
                case 65: return CoreResources.WeatherHeavyRain;
                case 66: return CoreResources.WeatherLightFreezingRain;
                case 67: return CoreResources.WeatherHeavyFreezingRain;
                case 71: return CoreResources.WeatherLightSnow;
                case 73: return CoreResources.WeatherModerateSnow;
                case 75: return CoreResources.WeatherHeavySnow;
                case 77: return CoreResources.WeatherSnowGrains;
                case 80: return CoreResources.WeatherLightRainShowers;
                case 81: return CoreResources.WeatherModerateRainShowers;
                case 82: return CoreResources.WeatherHeavyRainShowers;
                case 85: return CoreResources.WeatherLightSnowShowers;
                case 86: return CoreResources.WeatherHeavySnowShowers;
                case 95: return CoreResources.WeatherThunderstorm;
                case 96: return CoreResources.WeatherLightHail;
                case 99: return CoreResources.WeatherHeavyHail;
                default: return CoreResources.WeatherUnknown;
            }
        }

        public static string GetWeatherIcon(int weatherCode, bool isDay)
        {
            if (weatherCode <= 1) return IconBase + (isDay ? "clear-day.png" : "clear-night.png");
            string icon;
            return IconMap.TryGetValue(weatherCode, out icon) ? IconBase + icon + ".png" : IconBase + "not-available.png";
        }

        public static string GetWeatherTileIcon(int weatherCode, bool isDay)
        {
            if (weatherCode <= 1) return (isDay ? "clear-day.png" : "clear-night.png");
            string icon;
            return IconMap.TryGetValue(weatherCode, out icon) ? icon + ".png" : "not-available.png";
        }
    }
}