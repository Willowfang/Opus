using System.Globalization;
using System.Windows.Data;

namespace Opus.Common.Converters
{
    /// <summary>
    /// Converter for converting a number to boolean.
    /// </summary>
    public class EmptyToBoolConverter : IValueConverter
    {
        /// <summary>
        /// If value is number and equals 0, return false. Else return true.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>He
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((int)value == 0)
                return false;
            else
                return true;
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
