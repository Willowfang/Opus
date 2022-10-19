using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Opus.Styles.Converters
{
    /// <summary>
    /// Convert null / non-null values to string.
    /// </summary>
    public class NullToString : IValueConverter
    {
        /// <summary>
        /// If value is null but parameter is not, return parameter.
        /// If value is null and parameter is null, return empty string.
        /// If value is not null and is string, return it.
        /// If value is not null and is not string, return empty string.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
            {
                if (parameter is string placeholder)
                {
                    return placeholder;
                }

                return string.Empty;
            }

            if (value is string text)
            {
                return text;
            }

            return string.Empty;
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
