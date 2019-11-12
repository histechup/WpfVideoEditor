using WpfVideoEditor.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace WpfVideoEditor.Converters
{
    [ValueConversion(typeof(VideoCodec), typeof(int))]
    public class MediaVideoCodecValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (VideoCodec)value;
        }
    }
}
