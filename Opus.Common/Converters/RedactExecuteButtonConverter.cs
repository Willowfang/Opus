using Opus.Common.Wrappers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Opus.Common.Converters
{
    /// <summary>
    /// Converter for redaction execution button.
    /// </summary>
    public class RedactExecuteButtonConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool rangeselection = (bool)values[0];
            bool wordselection = (bool)values[1];
            bool startError = (bool)values[2];
            bool endError = (bool)values[3];
            bool wordsError = (bool)values[4];
            int fileCount = (int)values[5];

            if (!rangeselection && !wordselection) return false;

            if (rangeselection)
            {
                if (startError || endError) return false;
            }

            if (wordselection)
            {
                if (wordsError) return false;
            }

            if (fileCount < 1) return false;

            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
