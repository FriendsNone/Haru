using HaruCore;
using System.Collections.Generic;
using System.ComponentModel;

namespace HaruApp.ViewModels
{
    public class GeocodingViewModel : INotifyPropertyChanged
    {
        private List<LocationRecord> location;
        public List<LocationRecord> Location
        {
            get { return location; }
            set
            {
                if (location != value)
                {
                    location = value;
                    OnPropertyChanged("Location");
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
}