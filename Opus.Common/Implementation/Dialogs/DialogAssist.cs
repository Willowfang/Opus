using WF.LoggingLib;
using WF.PdfLib.Common;
using Opus.Common.Extensions;
using Opus.Common.Logging;
using Opus.Common.Services.Dialogs;
using Opus.Common.Progress;
using Opus.Common.Dialogs;

namespace Opus.Common.Implementation.Dialogs
{
    /// <summary>
    /// Default implementation of <see cref="IDialogAssist"/>.
    /// </summary>
    public class DialogAssist : LoggingCapable<DialogAssist>, IDialogAssist
    {
        private bool isShowing;

        /// <summary>
        /// If true, current dialog is showing.
        /// </summary>
        public bool IsShowing
        {
            get => isShowing;
            set => SetProperty(ref isShowing, value);
        }

        private IDialog? active;

        /// <summary>
        /// The dialog that is currently active.
        /// </summary>
        public IDialog? Active
        {
            get => active;
            set => SetProperty(ref active, value);
        }

        private List<IDialog> currentDialogs;

        /// <summary>
        /// Create a new instance of dialog assistant.
        /// </summary>
        /// <param name="logbook">Logging service.</param>
        public DialogAssist(ILogbook logbook) : base(logbook)
        {
            currentDialogs = new List<IDialog>();
        }

        /// <summary>
        /// Show a chosen dialog.
        /// <para>
        /// Given dialog will be added to the list of current dialogs. If there are more than one dialog on the
        /// list, next dialogs will be shown after this last one is closed.
        /// </para>
        /// </summary>
        /// <param name="dialog">Dialog to show.</param>
        /// <returns>An awaitable task. The task returns current dialog.</returns>
        /// <exception cref="ArgumentNullException">Thrown, if dialog is null.</exception>
        public async Task<IDialog> Show(IDialog dialog)
        {
            if (dialog == null)
            {
                logbook.Write($"Dialog to show was null.", LogLevel.Error);
                throw new ArgumentNullException(nameof(dialog));
            }

            logbook.Write($"Showing dialog '{dialog.DialogTitle ?? "untitled"}'.", LogLevel.Debug);

            currentDialogs.Insert(0, dialog);

            ActivateNext();

            await dialog.DialogClosed.Task;

            CloseDialog(dialog);

            return dialog;
        }

        /// <summary>
        /// Show a progress dialog.
        /// </summary>
        /// <param name="cancelSource">Source for progress cancellation.</param>
        /// <returns>Container containing the progress dialog and progress instance for controlling progress.</returns>
        public ProgressContainer ShowProgress(CancellationTokenSource cancelSource)
        {
            ProgressDialog dialog = new ProgressDialog(null, cancelSource)
            {
                TotalPercent = 0,
                Phase = ProgressPhase.Unassigned.GetResourceString()
            };

            Progress<ProgressReport> progress = new Progress<ProgressReport>(report =>
            {
                dialog.TotalPercent = report.Percentage;
                dialog.Phase = report.CurrentPhase.GetResourceString();
                dialog.Part = report.CurrentItem;
            });

            return new ProgressContainer(Show(dialog), dialog, progress);
        }

        private void ActivateNext()
        {
            Active = currentDialogs[0];

            IsShowing = true;
        }

        private void CloseDialog(IDialog dialog)
        {
            int index = currentDialogs.IndexOf(dialog);

            currentDialogs.Remove(dialog);

            logbook.Write($"Dialog '{dialog.DialogTitle ?? "untitled"}' closed.", LogLevel.Debug);

            // No other dialogs opened. Close the container.

            if (currentDialogs.Count < 1)
            {
                IsShowing = false;
            }

            // Show next opened dialog.

            else if (index == 0)
            {
                ActivateNext();
            }
        }
    }
}
