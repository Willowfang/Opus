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
    /// Converter for converting various values to a string.
    /// </summary>
    public class SegmentNameConverter : IMultiValueConverter
    {
        /// <summary>
        /// Convert various conditions to a string.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string displayName = null;
            if (values[0] is bool nameFromFile && nameFromFile)
            {
                if (parameter is string replacement)
                    displayName = replacement;
            }
            else
            {
                if (values[1] is string name)
                    displayName = name;
            }

            if (values[2] is bool compulsory)
            {
                if (displayName != null)
                    displayName = compulsory ? displayName : $"({displayName})";
            }
            return displayName;
        }

        /// <summary>
        /// Not implemented, goes one way only.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetTypes"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
