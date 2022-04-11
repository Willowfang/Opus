using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Opus.Styles.Converters
{
    public class BooleanToInt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int assignNumber = System.Convert.ToInt32(parameter);
            int currentNumber = System.Convert.ToInt32(value);
            return assignNumber == currentNumber;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int assignNumber = System.Convert.ToInt32(parameter);
            bool isChecked = System.Convert.ToBoolean(value);
            return isChecked ? assignNumber : Binding.DoNothing;
        }
    }
}
