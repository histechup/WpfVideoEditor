using System;
using System.Globalization;
using System.Windows.Data;

namespace WpfVideoEditor.Converters
{
    [ValueConversion(typeof(long), typeof(string))]
    public class PrettyFileSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return MyExtensions.FormatFileSize((long)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
