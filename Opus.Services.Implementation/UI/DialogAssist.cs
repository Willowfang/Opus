using Opus.Services.UI;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.UI
{
    public class DialogAssist : BindableBase, IDialogAssist
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

        public DialogAssist()
        {
            currentDialogs = new List<IDialog>();
        }

        public async Task<IDialog> Show(IDialog dialog)
        {
            if (dialog == null)
                throw new ArgumentNullException(nameof(dialog));

            currentDialogs.Insert(0, dialog);
            ActivateNext();
            await dialog.DialogClosed.Task;
            CloseDialog(dialog);
            return dialog;
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
