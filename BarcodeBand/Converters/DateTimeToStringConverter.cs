using System;
using Windows.UI.Xaml.Data;

namespace BarcodeBand.Converters
{
    public class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var time = (DateTime) value;

            return time.ToString("t");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
