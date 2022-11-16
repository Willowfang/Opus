using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Opus.Common.Converters
{
    /// <summary>
    /// Converter for converting a boolean to its negative visibility counterpart.
    /// </summary>
    public class BooleanToOppositeVisibility : IValueConverter
    {
        /// <summary>
        /// If value is false, return visible. Otherwise return collapsed.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value == true)
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
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
