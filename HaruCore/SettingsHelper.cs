using System.IO.IsolatedStorage;

namespace HaruCore
{
    public static class SettingsHelper
    {
        public static bool GetBool(IsolatedStorageSettings settings, string key, bool defaultValue)
        {
            return settings.Contains(key) ? (bool)settings[key] : defaultValue;
        }
    }
}
