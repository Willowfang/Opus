using System.ComponentModel;

namespace Opus.Services.Implementation.UI.Dialogs
{
    /// <summary>
    /// A dialog for adding or editing a composition title segment.
    /// </summary>
    public class CompositionTitleSegmentDialog : DialogBase, IDataErrorInfo
    {
        private string? segmentName;

        /// <summary>
        /// Name of the title segment.
        /// </summary>
        public string? SegmentName
        {
            get => segmentName;
            set => SetProperty(ref segmentName, value);
        }

        /// <summary>
        /// Create a dialog for adding or editing a composition title segment.
        /// </summary>
        /// <param name="dialogTitle"></param>
        public CompositionTitleSegmentDialog(string dialogTitle) : base(dialogTitle) { }

        /// <summary>
        /// Validation error. Always return null.
        /// </summary>
        public string? Error
        {
            get => null;
        }

        /// <summary>
        /// Validation.
        /// </summary>
        /// <param name="propertyName">Property to validate.</param>
        /// <returns></returns>
        public string this[string propertyName]
        {
            get
            {
                if (propertyName == nameof(SegmentName))
                {
                    if (string.IsNullOrEmpty(SegmentName))
                        return Resources.Validation.General.NameEmpty;
                }

                return string.Empty;
            }
        }
    }
}
