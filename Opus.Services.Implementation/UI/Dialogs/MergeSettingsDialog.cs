using CX.LoggingLib;
using Opus.Services.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.UI.Dialogs
{
    /// <summary>
    /// A dialog for choosing merge settings.
    /// </summary>
    public class MergeSettingsDialog : DialogBase, IDialog
    {
        private bool addPageNumbers;

        /// <summary>
        /// If true, add page numbers to final product.
        /// </summary>
        public bool AddPageNumbers
        {
            get => addPageNumbers;
            set { SetProperty(ref addPageNumbers, value); }
        }

        /// <summary>
        /// Create a new dialog for choosing merging settings.
        /// </summary>
        /// <param name="dialogTitle">Title for the dialog.</param>
        public MergeSettingsDialog(string dialogTitle) : base(dialogTitle) { }
    }
}
