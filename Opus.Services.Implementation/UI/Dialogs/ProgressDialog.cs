using CX.LoggingLib;
using CX.PdfLib.Common;
using Opus.Services.Implementation.UI;
using Opus.Services.UI;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.UI.Dialogs
{
    /// <summary>
    /// A dialog for showing progress to the user.
    /// </summary>
    public class ProgressDialog : DialogBase, IDialog
    {
        private int totalPercent;

        /// <summary>
        /// Total percentage done.
        /// </summary>
        public int TotalPercent
        {
            get => totalPercent;
            set => SetProperty(ref totalPercent, value);
        }

        private int partPercent;

        /// <summary>
        /// Percentage done of a sub-process.
        /// </summary>
        public int PartPercent
        {
            get => partPercent;
            set => SetProperty(ref partPercent, value);
        }

        private string? phase;

        /// <summary>
        /// Phase that is currently being done.
        /// </summary>
        public string? Phase
        {
            get => phase;
            set => SetProperty(ref phase, value);
        }

        private string? part;

        /// <summary>
        /// Part of a phase currently being done.
        /// </summary>
        public string? Part
        {
            get => part;
            set => SetProperty(ref part, value);
        }

        /// <summary>
        /// Source for cancellation.
        /// </summary>
        private CancellationTokenSource cancellationSource;

        /// <summary>
        /// Create a new dialog for showing progress to the user.
        /// </summary>
        /// <param name="dialogTitle">Title of the dialog.</param>
        /// <param name="cancellationSource">Source for cancellation.</param>
        public ProgressDialog(string? dialogTitle, CancellationTokenSource cancellationSource)
            : base(dialogTitle)
        {
            this.cancellationSource = cancellationSource;
        }

        /// <summary>
        /// Execution method for closing the dialog.
        /// </summary>
        protected override void ExecuteClose()
        {
            if (TotalPercent < 100)
            {
                cancellationSource.Cancel();
                base.ExecuteClose();
            }
            else
            {
                base.ExecuteSave();
            }
        }

        /// <summary>
        /// Method to execute when closing on error.
        /// </summary>
        public override void CloseOnError()
        {
            cancellationSource.Cancel();
            base.CloseOnError();
        }
    }
}
