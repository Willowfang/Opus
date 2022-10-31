using Opus.Services.UI;
using System.ComponentModel;

namespace Opus.Services.Implementation.UI.Dialogs
{
    /// <summary>
    /// A dialog for choosing a file title.
    /// </summary>
    public class FileTitleDialog : DialogBase, IDialog, IDataErrorInfo
    {
        private string? title;

        /// <summary>
        /// Title of the file.
        /// </summary>
        public string? Title
        {
            get => title;
            set
            {
                SetProperty(ref title, value);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Create a dialog for choosing a file title.
        /// </summary>
        /// <param name="dialogTitle">Title of the dialog.</param>
        public FileTitleDialog(string dialogTitle) : base(dialogTitle) { }

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
                if (propertyName == nameof(Title))
                {
                    if (string.IsNullOrWhiteSpace(Title))
                    {
                        return Resources.Validation.General.NameEmpty;
                    }
                }

                return string.Empty;
            }
        }
    }
}
