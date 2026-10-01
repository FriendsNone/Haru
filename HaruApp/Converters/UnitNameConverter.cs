using HaruApp.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace HaruApp.Converters
{
    public class UnitNameConverter : IValueConverter
    {
        private static readonly Dictionary<string, Func<string>> Names = new Dictionary<string, Func<string>>
        {
            { "celsius", () => AppResources.UnitCelsius },
            { "fahrenheit", () => AppResources.UnitFahrenheit },
            { "kmh", () => AppResources.UnitKmh },
            { "ms", () => AppResources.UnitMs },
            { "mph", () => AppResources.UnitMph },
            { "kn", () => AppResources.UnitKn },
            { "mm", () => AppResources.UnitMm },
            { "inch", () => AppResources.UnitInch }
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var unit = value as string;
            Func<string> name;
            return unit != null && Names.TryGetValue(unit, out name) ? name() : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
