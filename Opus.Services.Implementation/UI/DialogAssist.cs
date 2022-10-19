using CX.LoggingLib;
using CX.PdfLib.Common;
using Opus.Services.Extensions;
using Opus.Services.Implementation.Logging;
using Opus.Services.Implementation.UI.Dialogs;
using Opus.Services.UI;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.UI
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
                logbook.Write($"{nameof(IDialog)} to show was null.", LogLevel.Debug);
                throw new ArgumentNullException(nameof(dialog));
            }

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

        /// <summary>
        /// Activate the dialog in line.
        /// </summary>
        private void ActivateNext()
        {
            Active = currentDialogs[0];
            IsShowing = true;
        }

        /// <summary>
        /// Close a dialog.
        /// </summary>
        /// <param name="dialog">Dialog to close.</param>
        private void CloseDialog(IDialog dialog)
        {
            int index = currentDialogs.IndexOf(dialog);
            currentDialogs.Remove(dialog);
            if (currentDialogs.Count < 1)
            {
                IsShowing = false;
            }
            else if (index == 0)
            {
                ActivateNext();
            }
        }
    }
}
