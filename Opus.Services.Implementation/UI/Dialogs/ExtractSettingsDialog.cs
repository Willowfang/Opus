using Opus.Services.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.UI.Dialogs
{
    public class ExtractSettingsDialog : DialogBase, IDialog
    {
        private string? prefix;
        public string? Prefix
        {
            get => prefix;
            set
            {
                SetProperty(ref prefix, value);
            }
        }

        private string? suffix;
        public string? Suffix
        {
            get => suffix;
            set
            {
                SetProperty(ref suffix, value);
            }
        }

        private bool alwaysAsk;
        public bool AlwaysAsk
        {
            get => alwaysAsk;
            set => SetProperty(ref alwaysAsk, value);
        }

        public bool IsAsking { get; }

        public ExtractSettingsDialog(string dialogTitle, bool isAsking = false)
            : base(dialogTitle) 
        {
            IsAsking = isAsking;
        }
    }
}
