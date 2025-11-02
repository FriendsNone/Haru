using System;
using System.Linq;
using System.Reflection;
using Microsoft.Phone.Info;
using Microsoft.Phone.Shell;

namespace HaruCore
{
    public static class TileHelper
    {
        private static bool? _isWP8;

        public static bool IsWindowsPhone8()
        {
            if (_isWP8.HasValue)
                return _isWP8.Value;

            try
            {
                var version = Environment.OSVersion.Version;
                _isWP8 = version.Major >= 8;
            }
            catch
            {
                _isWP8 = false;
            }

            return _isWP8.Value;
        }

        public static void UpdateTile(string location, string temperature, string weatherDescription,
                                     string weatherIcon, string weatherTile, string time)
        {
            var tile = ShellTile.ActiveTiles.FirstOrDefault();
            if (tile == null) return;

            if (IsWindowsPhone8())
            {
                UpdateWP8Tile(tile, location, temperature, weatherDescription, weatherTile, time);
            }
            else
            {
                UpdateWP7Tile(tile, location, temperature, weatherDescription, weatherTile, time);
            }
        }

        private static void UpdateWP7Tile(ShellTile tile, string location, string temperature,
                                         string weatherDescription, string weatherTile, string time)
        {
            tile.Update(new StandardTileData
            {
                Title = location,
#if DEBUG
                Count = DateTime.Now.Minute,
#else
                Count = 0,
#endif
                BackgroundImage = new Uri("/Assets/WeatherIcons/Tile/" + weatherTile, UriKind.Relative),
                BackTitle = time,
                BackContent = string.Format("{0}\n{1}", temperature, weatherDescription)
            });
        }

        private static void UpdateWP8Tile(ShellTile tile, string location, string temperature,
                                         string weatherDescription, string weatherTile, string time)
        {
            try
            {
                var flipTileDataType = Type.GetType("Microsoft.Phone.Shell.FlipTileData, Microsoft.Phone");
                if (flipTileDataType == null)
                {
                    UpdateWP7Tile(tile, location, temperature, weatherDescription, weatherTile, time);
                    return;
                }

                var tileData = Activator.CreateInstance(flipTileDataType);

                SetProperty(tileData, "Title", location);
#if DEBUG
                SetProperty(tileData, "Count", DateTime.Now.Minute);
#else
                SetProperty(tileData, "Count", 0);
#endif
                SetProperty(tileData, "BackgroundImage", new Uri("/Assets/WeatherIcons/Tile/" + weatherTile, UriKind.Relative));

                SetProperty(tileData, "WideBackgroundImage", new Uri("/Assets/WeatherIcons/WideTile/" + weatherTile, UriKind.Relative));
                SetProperty(tileData, "WideBackContent", string.Format("{0}\n{1}", temperature, weatherDescription));

                SetProperty(tileData, "BackTitle", time);
                SetProperty(tileData, "BackContent", string.Format("{0}\n{1}", temperature, weatherDescription));
                //SetProperty(tileData, "BackBackgroundImage", new Uri("/Assets/Background.png", UriKind.Relative));

                //SetProperty(tileData, "WideBackBackgroundImage", new Uri("/Assets/Background.png", UriKind.Relative));

                SetProperty(tileData, "SmallBackgroundImage", new Uri("/Assets/small-tile.png", UriKind.Relative));

                var updateMethod = tile.GetType().GetMethod("Update", new[] { typeof(ShellTileData) });
                if (updateMethod != null)
                {
                    updateMethod.Invoke(tile, new[] { tileData });
                }
            }
            catch
            {
                UpdateWP7Tile(tile, location, temperature, weatherDescription, weatherTile, time);
            }
        }

        private static void SetProperty(object obj, string propertyName, object value)
        {
            var property = obj.GetType().GetProperty(propertyName);
            if (property != null && property.CanWrite)
            {
                property.SetValue(obj, value, null);
            }
        }
    }
}
