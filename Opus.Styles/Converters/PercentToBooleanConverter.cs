using System;
using System.Globalization;
using System.Windows.Data;

namespace Opus.Styles.Converters
{
    /// <summary>
    /// Converter for converting integer values to boolean.
    /// </summary>
    public class PercentToBooleanConverter : IValueConverter
    {
        /// <summary>
        /// If value is a number and parameter is a number and they are equal, return true.
        /// Otherwise, return false.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
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
