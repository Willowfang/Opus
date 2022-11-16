using System.Windows.Input;

namespace Opus.Common.Services.Dialogs
{
    /// <summary>
    /// A base definition for dialogs.
    /// </summary>
    public interface IDialog
    {
        /// <summary>
        /// Title of the dialog, usually shown in the top part.
        /// </summary>
        public string? DialogTitle { get; }

        /// <summary>
        /// If true, the dialog was cancelled.
        /// </summary>
        public bool IsCanceled { get; }

        /// <summary>
        /// Completion source indicating when the dialog is closed.
        /// </summary>
        public TaskCompletionSource DialogClosed { get; set; }

        /// <summary>
        /// Command for saving the information in the dialog.
        /// </summary>
        public ICommand Save { get; }

        /// <summary>
        /// Command for closing the dialog (is agnostic of saving).
        /// </summary>
        public ICommand Close { get; }

        /// <summary>
        /// Function for closing the dialog on error.
        /// </summary>
        public void CloseOnError();
    }
}
