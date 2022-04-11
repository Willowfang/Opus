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
    public class DialogAssist : LoggingCapable<DialogAssist>, IDialogAssist
    {
        private bool isShowing;
        public bool IsShowing
        {
            get => isShowing;
            set => SetProperty(ref isShowing, value);
        }

        private IDialog? active;
        public IDialog? Active
        {
            get => active;
            set => SetProperty(ref active, value);
        }

        private List<IDialog> currentDialogs;

        public DialogAssist(ILogbook logbook) : base(logbook)
        {
            currentDialogs = new List<IDialog>();
        }

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
