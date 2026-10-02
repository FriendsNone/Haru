using HaruApp.Resources;
using HaruCore;
using System.Collections.Generic;
using System.ComponentModel;

namespace HaruApp.ViewModels
{
    public class FavoritesViewModel : INotifyPropertyChanged
    {
        private List<FavoriteRecord> favorites;
        public List<FavoriteRecord> Favorites
        {
            get { return favorites; }
            set
            {
                if (favorites != value)
                {
                    favorites = value;
                    OnPropertyChanged("Favorites");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class FavoriteRecord
    {
        public Place Place { get; set; }
        public bool IsHome { get; set; }
        public bool IsPinned { get; set; }
        public bool CanRemove { get; set; }

        public string Name { get { return Place.Name; } }

        public string Detail
        {
            get { return IsHome ? string.Format(AppResources.FavoritesHomeDetail, Place.Coordinates) : Place.Coordinates; }
        }

        public bool CanPin { get { return IsHome || !IsPinned; } }
    }
}
