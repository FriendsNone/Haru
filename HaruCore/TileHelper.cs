using Mangopollo.Tiles;
using Microsoft.Phone.Shell;
using System;
using System.Linq;

namespace HaruCore
{
    public static class TileHelper
    {
        private const string LiveTileBase = "/Assets/LiveTile/";
        private const string SmallTileImage = "/Assets/ApplicationTileSmall.png";
        private const string DefaultTileImage = "/Assets/ApplicationTile.png";
        private const string DefaultTitle = "Haru";

        private static Uri TileUri(string name, bool wide, bool mono)
        {
            var suffix = (wide ? "_wide" : "") + (mono ? "_mono" : "");
            return new Uri(LiveTileBase + name + suffix + ".png", UriKind.Relative);
        }

        private static bool? _canUseFlipTile;

        private static bool CanUseFlipTile()
        {
            if (_canUseFlipTile.HasValue)
                return _canUseFlipTile.Value;

            var version = Environment.OSVersion.Version;
            _canUseFlipTile = version.Major >= 8 ||
                              (version.Major == 7 && version.Build >= 8858);

            return _canUseFlipTile.Value;
        }

        public static void ResetTile()
        {
            var tile = ShellTile.ActiveTiles.FirstOrDefault();
            if (tile == null) return;

            var defaultImage = new Uri(DefaultTileImage, UriKind.Relative);
            var clearImage = new Uri("", UriKind.Relative);

            if (CanUseFlipTile())
            {
                tile.Update(new FlipTileData
                {
                    Title = DefaultTitle,
                    Count = 0,
                    SmallBackgroundImage = new Uri(SmallTileImage, UriKind.Relative),
                    BackgroundImage = defaultImage,
                    WideBackgroundImage = clearImage,
                    BackTitle = string.Empty,
                    BackContent = string.Empty,
                    WideBackContent = string.Empty
                });
            }
            else
            {
                tile.Update(new StandardTileData
                {
                    Title = DefaultTitle,
                    Count = 0,
                    BackgroundImage = defaultImage,
                    BackTitle = string.Empty,
                    BackContent = string.Empty
                });
            }
        }

        public static void UpdateTile(string location, string temperature, string weatherDescription,
                                      string weatherIcon, string weatherTile, string time, bool mono = false)
        {
            var tile = ShellTile.ActiveTiles.FirstOrDefault();
            if (tile == null) return;

            if (CanUseFlipTile())
                UpdateFlipTile(tile, location, temperature, weatherDescription, weatherTile, time, mono);
            else
                UpdateStandardTile(tile, location, temperature, weatherDescription, weatherTile, time, mono);
        }

        private static void UpdateFlipTile(ShellTile tile, string location, string temperature,
                                           string weatherDescription, string weatherTile, string time, bool mono)
        {
            var data = new FlipTileData
            {
                Title = location,
#if DEBUG
                Count = DateTime.Now.Minute,
#else
                Count = 0,
#endif
                SmallBackgroundImage = new Uri(SmallTileImage, UriKind.Relative),
                BackgroundImage = TileUri(weatherTile, wide: false, mono: mono),
                WideBackgroundImage = TileUri(weatherTile, wide: true, mono: mono),
                BackTitle = time,
                BackContent = string.Format("{0}\n{1}", temperature, weatherDescription),
                WideBackContent = string.Format("{0}\n{1}", temperature, weatherDescription)
            };

            tile.Update(data);
        }

        private static void UpdateStandardTile(ShellTile tile, string location, string temperature,
                                               string weatherDescription, string weatherTile, string time, bool mono)
        {
            tile.Update(new StandardTileData
            {
                Title = location,
#if DEBUG
                Count = DateTime.Now.Minute,
#else
                Count = 0,
#endif
                BackgroundImage = TileUri(weatherTile, wide: false, mono: mono),
                BackTitle = time,
                BackContent = string.Format("{0}\n{1}", temperature, weatherDescription)
            });
        }
    }
}