using Newtonsoft.Json;
using System.Globalization;

namespace HaruCore
{
    public class Place
    {
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        [JsonIgnore]
        public string Key
        {
            get { return KeyFor(Latitude, Longitude); }
        }

        [JsonIgnore]
        public string Coordinates
        {
            get { return string.Format(CultureInfo.InvariantCulture, "{0:0.####}, {1:0.####}", Latitude, Longitude); }
        }

        public bool IsSameAs(Place other)
        {
            return other != null && other.Key == Key;
        }

        public static string KeyFor(double latitude, double longitude)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0.####}_{1:0.####}", latitude, longitude);
        }
    }
}
