using System.Globalization;
using System.Windows.Data;

namespace Opus.Common.Converters
{
    /// <summary>
    /// Converter for converting null / non-null values to boolean.
    /// </summary>
    public class NullToBoolConverter : IValueConverter
    {
        /// <summary>
        /// If value is null, return false. Otherwise return true.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Not implemented. Goes one way only.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
