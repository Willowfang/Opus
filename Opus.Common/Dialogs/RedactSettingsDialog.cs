using Opus.Common.Services.Dialogs;
using System.Collections.ObjectModel;
using System.Windows.Media;
using Opus.Common.Extensions;

namespace Opus.Common.Dialogs
{
    /// <summary>
    /// A dialog for choosing settings for redaction.
    /// </summary>
    public class RedactSettingsDialog : DialogBase, IDialog
    {
        /// <summary>
        /// A list of available colors for redactions.
        /// </summary>
        public List<SolidColorBrush> RedactColors { get; }

        private SolidColorBrush selectedOutline;
        /// <summary>
        /// Selected outline color.
        /// </summary>
        public SolidColorBrush SelectedOutline
        {
            get => selectedOutline;
            set => SetProperty(ref selectedOutline, value);
        }

        private SolidColorBrush selectedFill;
        /// <summary>
        /// Selected fill color.
        /// </summary>
        public SolidColorBrush SelectedFill
        {
            get => selectedFill;
            set => SetProperty(ref selectedFill, value);
        }

        private string suffix;
        /// <summary>
        /// Suffix for filenames.
        /// </summary>
        public string Suffix
        {
            get => suffix;
            set => SetProperty(ref suffix, value);
        }

        /// <summary>
        /// Set the selected outline color. Add color to list if it is new, otherwise
        /// select an existing color.
        /// </summary>
        /// <param name="hex">HTML hex for color.</param>
        public void SelectOutline(string hex)
        {
            SelectedOutline = SelectColor(hex);
        }


        /// <summary>
        /// Set the selected fill color. Add color to list if it is new, otherwise
        /// select an existing color.
        /// </summary>
        /// <param name="hex">HTML hex for color.</param>
        public void SelectFill(string hex)
        {
            SelectedFill = SelectColor(hex);
        }

        private SolidColorBrush SelectColor(string hex)
        {
            foreach (SolidColorBrush brush in RedactColors)
            {
                if (brush.Compare(hex.HtmlHexToBrush()))
                {
                    return brush;
                }
            }

            SolidColorBrush added = hex.HtmlHexToBrush();
            RedactColors.Add(added);

            return added;
        }

        /// <summary>
        /// Setting for redaction.
        /// </summary>
        /// <param name="dialogTitle">Title for dialog.</param>
        /// <param name="colors">Colors for redaction</param>
        public RedactSettingsDialog(
            string dialogTitle, 
            string[] colors) : base(dialogTitle) 
        {
            RedactColors = new List<SolidColorBrush>();
            
            if (colors == null || colors.Length == 0)
            {
                RedactColors.Add(new SolidColorBrush(Colors.Red));
                RedactColors.Add(new SolidColorBrush(Colors.Black));
            } 
            else
            {
                foreach (string color in colors)
                {
                    RedactColors.Add(color.HtmlHexToBrush());
                }
            }

            selectedOutline = RedactColors[0];
            selectedFill = RedactColors[1];

            suffix = Resources.DefaultValues.DefaultValues.RedactSuffix;
        }
    }
}
