using CX.LoggingLib;
using Opus.Services.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.UI.Dialogs
{
    public class MergeSettingsDialog : DialogBase, IDialog
    {
        private bool addPageNumbers;
        public bool AddPageNumbers
        {
            get => addPageNumbers;
            set
            {
                SetProperty(ref addPageNumbers, value);
            }
        }

        public MergeSettingsDialog(string dialogTitle)
            : base(dialogTitle) { }
    }
}
