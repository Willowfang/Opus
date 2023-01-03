using System.Globalization;
using System.Windows.Data;

namespace Opus.Common.Converters
{
    /// <summary>
    /// If value is null, return true. Otherwise, return false.
    /// </summary>
    public class NullToBoolInverse : IValueConverter
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
