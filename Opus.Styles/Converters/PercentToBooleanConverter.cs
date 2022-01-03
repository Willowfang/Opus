using System;
using System.Globalization;
using System.Windows.Data;

namespace Opus.Styles.Converters
{
    public class PercentToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((int)value == int.Parse((string)parameter))
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
