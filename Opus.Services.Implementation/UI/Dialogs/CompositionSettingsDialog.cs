using CX.LoggingLib;
using Opus.Services.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.UI.Dialogs
{
    public class CompositionSettingsDialog : DialogBase, IDialog
    {
        private bool searchSubDirectories;
        public bool SearchSubDirectories
        {
            get => searchSubDirectories;
            set => SetProperty(ref searchSubDirectories, value);
        }

        private bool deleteConverted;
        public bool DeleteConverted
        {
            get => deleteConverted;
            set => SetProperty(ref deleteConverted, value);
        }

        public CompositionSettingsDialog(string dialogTitle)
            : base(dialogTitle) { }
    }
}
