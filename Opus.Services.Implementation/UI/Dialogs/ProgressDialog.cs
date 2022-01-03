using Opus.Services.Implementation.UI;
using Opus.Services.UI;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.UI.Dialogs
{
    public class ProgressDialog : DialogBase, IDialog
    {
        private int totalPercent;
        public int TotalPercent
        {
            get => totalPercent;
            set => SetProperty(ref totalPercent, value);
        }

        private int partPercent;
        public int PartPercent
        {
            get => partPercent;
            set => SetProperty(ref partPercent, value);
        }

        private string? phase;
        public string? Phase
        {
            get => phase;
            set => SetProperty(ref phase, value);
        }

        private string? part;
        public string? Part
        {
            get => part;
            set => SetProperty(ref part, value);
        }

        public ProgressDialog(string dialogTitle)
            : base(dialogTitle) { }

        protected override void ExecuteClose()
        {
            if (TotalPercent < 100)
            {
                base.ExecuteClose();
            }
            else
            {
                base.ExecuteSave();
            }
        }
    }
}
