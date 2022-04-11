using CX.LoggingLib;
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
    public class MessageDialog : DialogBase, IDialog
    {
        private string? content;
        public string? Content
        {
            get => content;
            set => SetProperty(ref content, value);
        }

        public MessageDialog(string dialogTitle, string content)
            : base(dialogTitle)
        {
            Content = content;
        }
    }
}
