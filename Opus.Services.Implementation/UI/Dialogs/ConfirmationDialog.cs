using CX.LoggingLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.UI.Dialogs
{
    public class ConfirmationDialog : DialogBase
    {
        private string? content;
        public string? Content
        {
            get => content;
            set => SetProperty(ref content, value);
        }

        public ConfirmationDialog(string dialogTitle, string content)
            : base(dialogTitle)
        {
            Content = content;
        }
    }
}
