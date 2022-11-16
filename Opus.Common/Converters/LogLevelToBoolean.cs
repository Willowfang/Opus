using System.Globalization;
using System.Windows.Data;

namespace Opus.Common.Converters
{
    /// <summary>
    /// Compare value and parameter and if they match, return true.
    /// </summary>
    public class LogLevelToBoolean : IValueConverter
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null
                && parameter != null
                && value is int level)
            {
                int compare = Int32.Parse(parameter as string);

                if (level == compare) return true;
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
