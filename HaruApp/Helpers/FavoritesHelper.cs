using HaruApp.Resources;
using HaruCore;
using System;
using System.Linq;

namespace HaruApp.Helpers
{
    public static class FavoritesHelper
    {
        public static bool IsHome(Place place)
        {
            return place.IsSameAs(HaruSettings.HomePlace);
        }

        public static bool IsFavorite(Place place)
        {
            return HaruSettings.Favorites.Any(f => f.IsSameAs(place));
        }

        public static bool CanRemove(Place place)
        {
            var favorites = HaruSettings.Favorites;
            return favorites.Any(f => f.IsSameAs(place)) && (!IsHome(place) || favorites.Count > 1);
        }

        public static bool TryAdd(Place place)
        {
            var favorites = HaruSettings.Favorites;
            if (favorites.Any(f => f.IsSameAs(place)))
                return true;

            if (favorites.Count >= HaruSettings.MaxFavorites)
            {
                PromptHelper.ShowAlert(
                    AppResources.PromptFavoritesFullTitle,
                    string.Format(AppResources.PromptFavoritesFullMessage, HaruSettings.MaxFavorites),
                    AppResources.PromptOkay
                );
                return false;
            }

            favorites.Add(new Place { Name = place.Name, Latitude = place.Latitude, Longitude = place.Longitude });
            HaruSettings.Favorites = favorites;
            HaruSettings.Save();
            return true;
        }

        public static void Remove(Place place, Action onRemoved)
        {
            if (!CanRemove(place)) return;

            var tile = TileHelper.FindTile(place);
            if (tile == null)
            {
                RemoveNow(place);
                if (onRemoved != null) onRemoved();
                return;
            }

            PromptHelper.ShowPrompt(
                AppResources.PromptRemoveFavoriteTitle,
                AppResources.PromptRemoveFavoriteMessage,
                AppResources.PromptRemove,
                AppResources.PromptCancel,
                () =>
                {
                    tile.Delete();
                    RemoveNow(place);
                    if (onRemoved != null) onRemoved();
                });
        }

        public static void Pin(Place place, CurrentRecord current)
        {
            if (!IsHome(place))
            {
                if (TileHelper.FindTile(place) != null || !TryAdd(place))
                    return;
            }

            if (IsHome(place))
            {
                PromptHelper.ShowAlert(
                    AppResources.PromptPinHomeTitle,
                    string.Format(AppResources.PromptPinHomeMessage, place.Name),
                    AppResources.PromptOkay
                );
                return;
            }

            if (!HaruSettings.BackgroundUpdateEnabled || !HaruSettings.LiveTileEnabled)
                current = null;
            else if (current == null)
                current = GetCachedCurrent(place);

            TileHelper.CreateTile(place, current, HaruSettings.MonochromeTileEnabled);
        }

        private static void RemoveNow(Place place)
        {
            var wasHome = IsHome(place);

            HaruSettings.Favorites = HaruSettings.Favorites.Where(f => !f.IsSameAs(place)).ToList();
            HaruSettings.Save();

            if (wasHome)
                RefreshHomeTile();
        }

        private static void RefreshHomeTile()
        {
            var home = HaruSettings.HomePlace;
            if (home == null || !HaruSettings.BackgroundUpdateEnabled || !HaruSettings.LiveTileEnabled)
                return;

            TileHelper.UpdateTile(TileHelper.PrimaryTile, home.Name, GetCachedCurrent(home), HaruSettings.MonochromeTileEnabled);
        }

        private static CurrentRecord GetCachedCurrent(Place place)
        {
            var cached = OpenMeteoClient.GetCachedForecast(place.Latitude, place.Longitude);
            return cached == null ? null : cached.ToCurrentRecord();
        }
    }
}
