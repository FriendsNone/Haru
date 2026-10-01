using System.IO.IsolatedStorage;

namespace HaruCore
{
    public static class HaruSettings
    {
        public const string DefaultTemperatureUnit = "celsius";
        public const string DefaultWindSpeedUnit = "kmh";
        public const string DefaultPrecipitationUnit = "mm";

        private static IsolatedStorageSettings Store
        {
            get { return IsolatedStorageSettings.ApplicationSettings; }
        }

        public static bool HasLocation
        {
            get
            {
                return Store.Contains(SettingsKeys.Location)
                    && Store.Contains(SettingsKeys.Latitude)
                    && Store.Contains(SettingsKeys.Longitude);
            }
        }

        public static string Location
        {
            get { return Get<string>(SettingsKeys.Location, null); }
            set { Store[SettingsKeys.Location] = value; }
        }

        public static double Latitude
        {
            get { return Get(SettingsKeys.Latitude, 0.0); }
            set { Store[SettingsKeys.Latitude] = value; }
        }

        public static double Longitude
        {
            get { return Get(SettingsKeys.Longitude, 0.0); }
            set { Store[SettingsKeys.Longitude] = value; }
        }

        // Unit values are raw Open-Meteo query values, passed straight to the API.
        public static string TemperatureUnit
        {
            get { return Get(SettingsKeys.TemperatureUnit, DefaultTemperatureUnit); }
            set { Store[SettingsKeys.TemperatureUnit] = value; }
        }

        public static string WindSpeedUnit
        {
            get { return Get(SettingsKeys.WindSpeedUnit, DefaultWindSpeedUnit); }
            set { Store[SettingsKeys.WindSpeedUnit] = value; }
        }

        public static string PrecipitationUnit
        {
            get { return Get(SettingsKeys.PrecipitationUnit, DefaultPrecipitationUnit); }
            set { Store[SettingsKeys.PrecipitationUnit] = value; }
        }

        public static bool BackgroundUpdateEnabled
        {
            get { return Get(SettingsKeys.BackgroundUpdateEnable, true); }
            set { Store[SettingsKeys.BackgroundUpdateEnable] = value; }
        }

        public static bool LiveTileEnabled
        {
            get { return Get(SettingsKeys.LiveTileEnable, true); }
            set { Store[SettingsKeys.LiveTileEnable] = value; }
        }

        public static bool NotificationEnabled
        {
            get { return Get(SettingsKeys.NotificationEnable, true); }
            set { Store[SettingsKeys.NotificationEnable] = value; }
        }

        public static bool MonochromeTileEnabled
        {
            get { return Get(SettingsKeys.MonochromeTileEnable, false); }
            set { Store[SettingsKeys.MonochromeTileEnable] = value; }
        }

        public static bool LocationServiceEnabled
        {
            get { return Get(SettingsKeys.LocationServiceEnable, false); }
            set { Store[SettingsKeys.LocationServiceEnable] = value; }
        }

        public static bool FirstTimeLocationShown
        {
            get { return Get(SettingsKeys.FirstTimeLocation, false); }
            set { SetFlag(SettingsKeys.FirstTimeLocation, value); }
        }

        public static bool BackgroundAgentDisabledShown
        {
            get { return Get(SettingsKeys.BackgroundAgentDisabledShown, false); }
            set { SetFlag(SettingsKeys.BackgroundAgentDisabledShown, value); }
        }

        public static void Save()
        {
            Store.Save();
        }

        private static T Get<T>(string key, T defaultValue)
        {
            object value;
            return Store.TryGetValue(key, out value) && value is T ? (T)value : defaultValue;
        }

        private static void SetFlag(string key, bool value)
        {
            if (value)
                Store[key] = true;
            else
                Store.Remove(key);
        }
    }
}
