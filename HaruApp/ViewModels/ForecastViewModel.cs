using HaruCore;
using System.Collections.Generic;
using System.ComponentModel;

namespace HaruApp.ViewModels
{
    public class ForecastViewModel : INotifyPropertyChanged
    {
        private CurrentRecord current;
        public CurrentRecord Current
        {
            get { return current; }
            set { if (current != value) { current = value; OnPropertyChanged("Current"); } }
        }

        private List<HourlyRecord> hourly;
        public List<HourlyRecord> Hourly
        {
            get { return hourly; }
            set { if (hourly != value) { hourly = value; OnPropertyChanged("Hourly"); } }
        }

        private List<DailyRecord> daily;
        public List<DailyRecord> Daily
        {
            get { return daily; }
            set { if (daily != value) { daily = value; OnPropertyChanged("Daily"); } }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}