using Mangopollo.Tiles;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace HaruCore
{
    public static class TileHelper
    {
        private const string LiveTileBase = "/Assets/LiveTile/";
        private const string SmallTileImage = "/Assets/ApplicationTileSmall.png";
        private const string DefaultTileImage = "/Assets/ApplicationTile.png";
        private const string DefaultTitle = "Haru";
        private const string TilePage = "/Views/MainPage.xaml";

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

        public static ShellTile PrimaryTile
        {
            get { return ShellTile.ActiveTiles.FirstOrDefault(); }
        }

        public static Uri NavigationUriFor(Place place)
        {
            return new Uri(string.Format(CultureInfo.InvariantCulture, "{0}?location={1}&lat={2:R}&lon={3:R}",
                TilePage, Uri.EscapeDataString(place.Name), place.Latitude, place.Longitude), UriKind.Relative);
        }

        public static Place ParsePlace(IDictionary<string, string> query)
        {
            string name, lat, lon;
            double latitude, longitude;

            if (!query.TryGetValue("location", out name) || string.IsNullOrEmpty(name)
                || !query.TryGetValue("lat", out lat) || !query.TryGetValue("lon", out lon)
                || !double.TryParse(lat, NumberStyles.Float, CultureInfo.InvariantCulture, out latitude)
                || !double.TryParse(lon, NumberStyles.Float, CultureInfo.InvariantCulture, out longitude))
                return null;

            return new Place { Name = name, Latitude = latitude, Longitude = longitude };
        }

        public static Place GetPlace(ShellTile tile)
        {
            var uri = tile.NavigationUri == null ? null : tile.NavigationUri.OriginalString;
            if (uri == null || !uri.StartsWith(TilePage + "?", StringComparison.OrdinalIgnoreCase))
                return null;

            var query = new Dictionary<string, string>();
            foreach (var pair in uri.Substring(TilePage.Length + 1).Split('&'))
            {
                var separator = pair.IndexOf('=');
                if (separator > 0)
                    query[pair.Substring(0, separator)] = Uri.UnescapeDataString(pair.Substring(separator + 1));
            }
            return ParsePlace(query);
        }

        public static List<Place> GetPinnedPlaces()
        {
            return ShellTile.ActiveTiles.Select(GetPlace).Where(p => p != null).ToList();
        }

        public static ShellTile FindTile(Place place)
        {
            return ShellTile.ActiveTiles.FirstOrDefault(t => place.IsSameAs(GetPlace(t)));
        }

        public static void CreateTile(Place place, CurrentRecord current, bool mono)
        {
            var uri = NavigationUriFor(place);

            if (CanUseFlipTile())
                ShellTileExt.Create(uri, BuildFlipTileData(place.Name, current, mono), true);
            else
                ShellTile.Create(uri, BuildStandardTileData(place.Name, current, mono));
        }

        public static void ResetTile()
        {
            var home = HaruSettings.HomePlace;
            ResetTile(PrimaryTile, home != null ? home.Name : DefaultTitle);
        }

        public static void ResetTile(ShellTile tile, string title)
        {
            UpdateTile(tile, title, null, false);
        }

        public static void ResetAllTiles()
        {
            ResetTile();
            foreach (var tile in ShellTile.ActiveTiles)
            {
                var place = GetPlace(tile);
                if (place != null)
                    ResetTile(tile, place.Name);
            }
        }

        public static void UpdateTile(ShellTile tile, string title, CurrentRecord current, bool mono = false)
        {
            if (tile == null) return;

            if (CanUseFlipTile())
                tile.Update(BuildFlipTileData(title, current, mono));
            else
                tile.Update(BuildStandardTileData(title, current, mono));
        }

        private static int TileCount
        {
#if DEBUG
            get { return DateTime.Now.Minute; }
#else
            get { return 0; }
#endif
        }

        private static FlipTileData BuildFlipTileData(string title, CurrentRecord current, bool mono)
        {
            if (current == null)
            {
                return new FlipTileData
                {
                    Title = title,
                    Count = 0,
                    SmallBackgroundImage = new Uri(SmallTileImage, UriKind.Relative),
                    BackgroundImage = new Uri(DefaultTileImage, UriKind.Relative),
                    WideBackgroundImage = new Uri("", UriKind.Relative),
                    BackTitle = string.Empty,
                    BackContent = string.Empty,
                    WideBackContent = string.Empty
                };
            }

            var content = string.Format("{0}\n{1}", current.Temperature, current.WeatherDescription);
            return new FlipTileData
            {
                Title = title,
                Count = TileCount,
                SmallBackgroundImage = new Uri(SmallTileImage, UriKind.Relative),
                BackgroundImage = TileUri(current.WeatherTile, wide: false, mono: mono),
                WideBackgroundImage = TileUri(current.WeatherTile, wide: true, mono: mono),
                BackTitle = UnitHelper.FormatObservationTime(current.ObservedUtc),
                BackContent = content,
                WideBackContent = content
            };
        }

        private static StandardTileData BuildStandardTileData(string title, CurrentRecord current, bool mono)
        {
            if (current == null)
            {
                return new StandardTileData
                {
                    Title = title,
                    Count = 0,
                    BackgroundImage = new Uri(DefaultTileImage, UriKind.Relative),
                    BackTitle = string.Empty,
                    BackContent = string.Empty
                };
            }

            return new StandardTileData
            {
                Title = title,
                Count = TileCount,
                BackgroundImage = TileUri(current.WeatherTile, wide: false, mono: mono),
                BackTitle = UnitHelper.FormatObservationTime(current.ObservedUtc),
                BackContent = string.Format("{0}\n{1}", current.Temperature, current.WeatherDescription)
            };
        }
    }
}
