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
    /// Converter for converting the comparison of two int values to boolean.
    /// </summary>
    public class BooleanToInt : IValueConverter
    {
        /// <summary>
        /// If parameter and value are both numbers and equal to each other, return true. Otherwise
        /// return false.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int assignNumber = System.Convert.ToInt32(parameter);
            int currentNumber = System.Convert.ToInt32(value);
            return assignNumber == currentNumber;
        }

        /// <summary>
        /// If value is boolean and true, return parameter as int. Otherwise, return an
        /// empty binding.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int assignNumber = System.Convert.ToInt32(parameter);
            bool isChecked = System.Convert.ToBoolean(value);
            return isChecked ? assignNumber : Binding.DoNothing;
        }
    }
}
