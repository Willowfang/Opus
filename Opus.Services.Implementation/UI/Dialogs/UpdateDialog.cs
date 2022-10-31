using Opus.Services.UI;
using System.Collections.Generic;
using System.Linq;

namespace Opus.Services.Implementation.UI.Dialogs
{
    /// <summary>
    /// A dialog for showing program update info.
    /// </summary>
    public class UpdateDialog : DialogBase, IDialog
    {
        /// <summary>
        /// Update info as a message to show.
        /// </summary>
        public string UpdateMessage { get; }

        /// <summary>
        /// Confirmation for update installation.
        /// </summary>
        public string UpdateConfirmation { get; }

        /// <summary>
        /// Notes for the update.
        /// </summary>
        public List<string> Notes { get; }

        /// <summary>
        /// If true, update notes will be shown.
        /// </summary>
        public bool ShowNotes { get; }

        /// <summary>
        /// Create a new dialog for showing info on updates and choosing whether to install them.
        /// </summary>
        /// <param name="dialogTitle">Title of the dialog.</param>
        /// <param name="version">New version number.</param>
        /// <param name="notes">Notes for the update.</param>
        public UpdateDialog(string dialogTitle, string version, string[] notes) : base(dialogTitle)
        {
            UpdateMessage = string.Format(Resources.Messages.StartUp.Update, version);
            UpdateConfirmation = Resources.Messages.StartUp.UpdateConfirm;
            Notes = notes.ToList();
            ShowNotes = notes.Length > 0;
        }
    }
}
