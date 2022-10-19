using CX.LoggingLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.UI.Dialogs
{
    /// <summary>
    /// A dialog for confirming an action.
    /// </summary>
    public class ConfirmationDialog : DialogBase
    {
        private string? content;

        /// <summary>
        /// Content to display to the user.
        /// </summary>
        public string? Content
        {
            get => content;
            set => SetProperty(ref content, value);
        }

        /// <summary>
        /// Create a new confirmation dialog.
        /// </summary>
        /// <param name="dialogTitle">Title of the dialog.</param>
        /// <param name="content">Content displayed to the user.</param>
        public ConfirmationDialog(string dialogTitle, string content) : base(dialogTitle)
        {
            Content = content;
        }
    }
}
