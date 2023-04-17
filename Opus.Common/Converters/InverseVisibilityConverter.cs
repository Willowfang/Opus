using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Opus.Common.Converters
{
    public class InverseVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                if (visibility == Visibility.Visible) return Visibility.Collapsed;

                if (visibility == Visibility.Hidden || visibility == Visibility.Collapsed) 
                    return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
