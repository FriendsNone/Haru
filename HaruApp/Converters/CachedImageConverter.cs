using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace HaruApp.Converters
{
    /// <summary>
    /// Returns a shared <see cref="BitmapImage"/> per image URI so that repeated weather
    /// icons (e.g. several "rain" rows) reuse a single decode instead of decoding the
    /// full-size PNG once per Image element. DecodePixelWidth/Height are intentionally
    /// left unset: the source stays at full resolution and is downscaled at render time,
    /// which keeps icons sharp on high-DPI (720p / WXGA / 1080p) devices.
    /// </summary>
    public class CachedImageConverter : IValueConverter
    {
        private static readonly Dictionary<string, BitmapImage> Cache =
            new Dictionary<string, BitmapImage>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = value as string;
            if (string.IsNullOrEmpty(path))
                return null;

            BitmapImage image;
            if (!Cache.TryGetValue(path, out image))
            {
                image = new BitmapImage(new Uri(path, UriKind.Relative));
                Cache[path] = image;
            }
            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
