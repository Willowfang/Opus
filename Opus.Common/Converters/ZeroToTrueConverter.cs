using System.Globalization;
using System.Windows.Data;

namespace Opus.Common.Converters
{
    /// <summary>
    /// Convert number to boolean.
    /// </summary>
    public class ZeroToTrueConverter : IValueConverter
    {
        /// <summary>
        /// If value is 0, return true. Otherwise, return false.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((int)value == 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Not implemented, goes one way only.
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
