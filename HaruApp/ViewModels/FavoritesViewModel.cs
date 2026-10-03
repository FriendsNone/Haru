using HaruApp.Resources;
using HaruCore;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

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

    public class FavoriteRecord : INotifyPropertyChanged
    {
        private const string Placeholder = "—";

        public Place Place { get; set; }
        public bool IsHome { get; set; }
        public bool IsPinned { get; set; }
        public bool CanRemove { get; set; }

        private CurrentRecord current;
        public CurrentRecord Current
        {
            get { return current; }
            set
            {
                if (current != value)
                {
                    current = value;
                    OnPropertyChanged("WeatherIcon");
                    OnPropertyChanged("WeatherDescription");
                    OnPropertyChanged("Temperature");
                    OnPropertyChanged("Humidity");
                    OnPropertyChanged("Precipitation");
                    OnPropertyChanged("WindSpeed");
                }
            }
        }

        public string Header
        {
            get
            {
                var header = IsHome ? string.Format(AppResources.FavoritesHomeDetail, Place.Name) : Place.Name;
                return header.ToUpper(CultureInfo.CurrentCulture);
            }
        }

        public string WeatherIcon
        {
            get { return Current != null ? Current.WeatherIcon : UnitHelper.GetWeatherIcon(null, true); }
        }

        public string WeatherDescription { get { return Current != null ? Current.WeatherDescription : Placeholder; } }
        public string Temperature { get { return Current != null ? Current.Temperature : Placeholder; } }
        public string Humidity { get { return Current != null ? Current.Humidity : Placeholder; } }
        public string Precipitation { get { return Current != null ? Current.Precipitation : Placeholder; } }
        public string WindSpeed { get { return Current != null ? Current.WindSpeed : Placeholder; } }

        public bool CanPin { get { return IsHome || !IsPinned; } }
        public bool CanSetHome { get { return !IsHome; } }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
