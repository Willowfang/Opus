using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Opus.Styles.Converters
{
    /// <summary>
    /// Converter for converting a given username to fontweight.
    /// </summary>
    public class UserNameToFontWeight : IValueConverter
    {
        /// <summary>
        /// If value is system user name, return demibold fontweight. Otherwise return normal fontweight.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not null &&
                value is string content)
            {
                if (content == Environment.UserName)
                    return FontWeights.DemiBold;
                else
                    return FontWeights.Normal;
            }
            else
                return FontWeights.Normal;
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
