using Opus.Services.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.UI.Dialogs
{
    public class UpdateDialog : DialogBase, IDialog
    {
        public string UpdateMessage { get; }
        public string UpdateConfirmation { get; }
        public List<string> Notes { get; }
        public bool ShowNotes { get; }

        public UpdateDialog(string dialogTitle, string version, string[] notes) : base(dialogTitle)
        {
            UpdateMessage = string.Format(Resources.Messages.StartUp.Update, version);
            UpdateConfirmation = Resources.Messages.StartUp.UpdateConfirm;
            Notes = notes.ToList();
            ShowNotes = notes.Length > 0;
        }
    }
}
