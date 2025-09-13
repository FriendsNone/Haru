using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HaruCore
{
    public class UnitModel
    {
        public static readonly Dictionary<string, string> TemperatureUnits = new Dictionary<string, string>
        {
            { "celsius", "°C" },
            { "fahrenheit", "°F" }
        };

        public static readonly Dictionary<string, string> WindSpeedUnits = new Dictionary<string, string>
        {
            { "kmh", "km/h" },
            { "ms", "m/s" },
            { "mph", "mph" },
            { "kn", "kn" }
        };

        public static readonly Dictionary<string, string> PrecipitationUnits = new Dictionary<string, string>
        {
            { "mm", "mm" },
            { "inch", "inch" }
        };

        private static readonly string[] Directions =
        {
            "north",
            "northeast",
            "east",
            "southeast",
            "south",
            "southwest",
            "west",
            "northwest"
        };

        public static string GetTemperatureUnit(string key)
        {
            if (TemperatureUnits.ContainsKey(key))
                return TemperatureUnits[key];
            return "°C";
        }

        public static string GetWindSpeedUnit(string key)
        {
            if (WindSpeedUnits.ContainsKey(key))
                return WindSpeedUnits[key];
            return "km/h";
        }

        public static string GetPrecipitationUnit(string key)
        {
            if (PrecipitationUnits.ContainsKey(key))
                return PrecipitationUnits[key];
            return "mm";
        }

        public static string InterpretDirection(double azimuth)
        {
            azimuth = (azimuth % 360 + 360) % 360;
            int index = (int)Math.Round(azimuth / 45.0) % 8;
            return Directions[index];
        }

        public static string InterpretTimeDifference(string dateTime)
        {
            DateTime now = DateTime.Now;
            DateTime dt = DateTime.Parse(dateTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
            TimeSpan diff = now - dt;

            if (diff.TotalSeconds < 60)
                return "now";
            else if (diff.TotalMinutes < 60)
                return string.Format("{0} minute{1} ago", (int)diff.TotalMinutes, diff.TotalMinutes >= 2 ? "s" : "");
            else if (diff.TotalHours < 24)
                return string.Format("{0} hour{1} ago", (int)diff.TotalHours, diff.TotalHours >= 2 ? "s" : "");
            else
                return dt.ToString("t", System.Globalization.CultureInfo.CurrentCulture);
        }
    }
}
