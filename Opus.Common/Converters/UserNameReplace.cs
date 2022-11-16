using System.Globalization;
using System.Windows.Data;

namespace Opus.Common.Converters
{
    /// <summary>
    /// Converter for returning the system username or another name.
    /// </summary>
    public class UserNameReplace : IValueConverter
    {
        /// <summary>
        /// If value is system username, return a replacement string. Otherwise return value string.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not null && 
                value is string name &&
                parameter is not null &&
                parameter is string replacement)
            {
                if (name == Environment.UserName)
                    return replacement;
                else
                    return name;
            }
            else
                return value;
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
