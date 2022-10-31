using WF.PdfLib.Common;
using System;
using System.Threading.Tasks;

namespace Opus.Services.UI
{
    /// <summary>
    /// Class for storing information on progress of a task / tasks and the progress dialog associated with said task.
    /// </summary>
    public class ProgressContainer
    {
        /// <summary>
        /// Show the dialog.
        /// </summary>
        public Task Show { get; }

        /// <summary>
        /// Get the progress dialog associated with the task.
        /// </summary>
        public IDialog ProgressDialog { get; }

        /// <summary>
        /// Get the progress reporting instance for modifying progress.
        /// </summary>
        public IProgress<ProgressReport> Reporting { get; }

        /// <summary>
        /// Create a new container for progress reporting and displaying.
        /// </summary>
        /// <param name="show">Task retrieved from showing the dialog.</param>
        /// <param name="progressDialog">The progress dialog.</param>
        /// <param name="reporting">Reporting instance.</param>
        public ProgressContainer(
            Task show,
            IDialog progressDialog,
            IProgress<ProgressReport> reporting
        )
        {
            Show = show;
            ProgressDialog = progressDialog;
            Reporting = reporting;
        }
    }
}
