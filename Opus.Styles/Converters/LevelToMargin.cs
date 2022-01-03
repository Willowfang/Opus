using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Opus.Styles.Converters
{
    public class LevelToMargin : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int adjustedLevel = (int)value - 1;
            return new Thickness(adjustedLevel * 20, 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
