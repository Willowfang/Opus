﻿using Opus.Common.Progress;

namespace Opus.Common.Services.Dialogs
{
    /// <summary>
    /// Definition for classes handling dialog-related tasks.
    /// </summary>
    public interface IDialogAssist
    {
        /// <summary>
        /// Indicates whether the dialog is visible
        /// </summary>
        public bool IsShowing { get; }

        /// <summary>
        /// Currently visible dialog
        /// </summary>
        public IDialog? Active { get; }

        /// <summary>
        /// Show a dialog asynchronously and return it when ready
        /// </summary>
        /// <param name="dialog">Dialog to show</param>
        public Task<IDialog> Show(IDialog dialog);

        /// <summary>
        /// Show a progress dialog and return the associated task and dialog as well as the associated IProgress
        /// for reporting progress to the dialog.
        /// </summary>
        /// <param name="cancelSource">Cancellation source mainly for user cancellation interaction</param>
        /// <returns></returns>
        public ProgressContainer ShowProgress(CancellationTokenSource cancelSource);
    }
}
