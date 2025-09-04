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
    }
}
