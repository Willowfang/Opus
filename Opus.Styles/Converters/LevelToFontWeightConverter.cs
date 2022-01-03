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
    public class LevelToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int level = (int)value;
            if (level == 1) return FontWeights.Medium;
            if (level == 2) return FontWeights.Regular;
            if (level == 3) return FontWeights.Light;
            if (level == 4) return FontWeights.UltraLight;
            else return FontWeights.Thin;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
