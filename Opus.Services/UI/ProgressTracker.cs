using System;
using System.Threading;
using System.Threading.Tasks;
using WF.PdfLib.Common;

namespace Opus.Services.UI
{
    /// <summary>
    /// A class to track progress for an action. Shows a progress dialog.
    /// </summary>
    public class ProgressTracker
    {
        private int grandTotal;

        private int currentTotal;

        private ProgressContainer container;

        private ProgressPhase currentPhase;

        /// <summary>
        /// Source for the cancellation token.
        /// </summary>
        public CancellationTokenSource TokenSource { get; }

        /// <summary>
        /// Cancellation token for this progress.
        /// </summary>
        public CancellationToken Token { get; }

        /// <summary>
        /// Get the progress interface.
        /// </summary>
        public IProgress<ProgressReport> ProgressInterface
        {
            get => container.Reporting;
        }

        /// <summary>
        /// If true, this progress has been canceled.
        /// </summary>
        public bool IsCanceled
        {
            get => container.ProgressDialog.IsCanceled;
        }

        /// <summary>
        /// Get the progress dialog show task.
        /// </summary>
        public Task Show
        {
            get => container.Show;
        }

        /// <summary>
        /// Create new progress instance.
        /// </summary>
        /// <param name="grandTotal">When this is reached, progress is completed.</param>
        /// <param name="dialogAssist">Dialog service.</param>
        public ProgressTracker(int grandTotal, IDialogAssist dialogAssist)
        {
            this.grandTotal = grandTotal;

            currentTotal = 0;

            TokenSource = new CancellationTokenSource();

            Token = TokenSource.Token;

            currentPhase = ProgressPhase.Unassigned;

            container = dialogAssist.ShowProgress(TokenSource);
        }

        /// <summary>
        /// Update current progress.
        /// </summary>
        /// <param name="addToCurrentTotal">Amount to add to current total.</param>
        /// <param name="phase">Current phase in the progress. If no phase is provided,
        /// currently used phase is preserved.</param>
        public void Update(int addToCurrentTotal, ProgressPhase? phase = null)
        {
            currentTotal += addToCurrentTotal;

            ProgressReport report = new ProgressReport(GetPercentage(), phase ?? currentPhase);

            currentPhase = phase ?? currentPhase;

            container.Reporting.Report(report);
        }

        /// <summary>
        /// Set percentage to a fixed value. Retains current total and grand total.
        /// </summary>
        /// <param name="percentage">Percentage to set to.</param>
        public void SetPercentage(int percentage)
        {
            ProgressReport report = new ProgressReport(percentage, currentPhase);

            container.Reporting.Report(report);
        }

        public void SetToUndefined()
        {
            ProgressReport report = new ProgressReport(0, ProgressPhase.Unassigned);

            container.Reporting.Report(report);
        }

        public void SetToComplete()
        {
            ProgressReport report = new ProgressReport(100, ProgressPhase.Finished);

            container.Reporting.Report(report);
        }

        /// <summary>
        /// Cancel the progress.
        /// </summary>
        public void Cancel()
        {
            container.ProgressDialog.Close.Execute(null);
        }

        private int GetPercentage()
        {
            return currentTotal * 100 / grandTotal;
        }
    }
}
