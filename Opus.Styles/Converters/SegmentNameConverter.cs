using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Opus.Styles.Converters
{
    public class SegmentNameConverter : IMultiValueConverter
    {
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

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
