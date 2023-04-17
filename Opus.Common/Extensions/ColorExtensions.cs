using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Opus.Common.Extensions
{
    public static class ColorExtensions
    {
        public static string BrushToHtmlHex(this SolidColorBrush original)
        {
            System.Drawing.Color color = System.Drawing.Color.FromArgb(
                original.Color.A, 
                original.Color.R, 
                original.Color.G, 
                original.Color.B);

            return System.Drawing.ColorTranslator.ToHtml(color);
        }

        public static SolidColorBrush HtmlHexToBrush(this string original)
        {
            System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(original);
            return new SolidColorBrush(Color.FromRgb(color.R, color.G, color.B));
        }

        public static bool Compare(this SolidColorBrush original, SolidColorBrush other)
        {
            return original.Color.R == other.Color.R
                && original.Color.B == other.Color.B
                && original.Color.G == other.Color.G;
        }
    }
}
