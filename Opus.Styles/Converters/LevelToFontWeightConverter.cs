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
    /// Converter for converting a number value to fontweight.
    /// </summary>
    public class LevelToFontWeightConverter : IValueConverter
    {
        /// <summary>
        /// Convert number value (1 medium, 2 regular, 3 light, 4 ultralight, others thin) to font weight.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int level = (int)value;
            if (level == 1) return FontWeights.Medium;
            if (level == 2) return FontWeights.Regular;
            if (level == 3) return FontWeights.Light;
            if (level == 4) return FontWeights.UltraLight;
            else return FontWeights.Thin;
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
