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

        private CancellationTokenSource cancellationSource;

        public ProgressDialog(string dialogTitle, CancellationTokenSource cancellationSource)
            : base(dialogTitle) 
        {
            this.cancellationSource = cancellationSource;
        }

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
    }
}
