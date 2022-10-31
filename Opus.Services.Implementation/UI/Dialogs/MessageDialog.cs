using Opus.Services.UI;

namespace Opus.Services.Implementation.UI.Dialogs
{
    /// <summary>
    /// A dialog for showing a message to the user.
    /// </summary>
    public class MessageDialog : DialogBase, IDialog
    {
        private string? content;

        /// <summary>
        /// Message content.
        /// </summary>
        public string? Content
        {
            get => content;
            set => SetProperty(ref content, value);
        }

        /// <summary>
        /// Create a new dialog for showing a message to user.
        /// </summary>
        /// <param name="dialogTitle">Title of the dialog.</param>
        /// <param name="content">Message to show.</param>
        public MessageDialog(string dialogTitle, string content) : base(dialogTitle)
        {
            Content = content;
        }
    }
}
