using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Opus.Common.Converters
{
    /// <summary>
    /// Converter for converting hierarchical levels to margins.
    /// </summary>
    public class LevelToMargin : IValueConverter
    {
        /// <summary>
        /// Return a margin with left edge as level * 20 px.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int adjustedLevel = (int)value - 1;
            return new Thickness(adjustedLevel * 20, 0, 0, 0);
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
