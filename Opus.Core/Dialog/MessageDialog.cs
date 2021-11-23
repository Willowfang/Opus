using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Core.Dialog
{
    public class MessageDialog : BindableBase, IDialog
    {
        private string content;
        public string Content
        {
            get => content;
            set => SetProperty(ref content, value);
        }

        public MessageDialog(string content = null)
        {
            Content = content;
        }
    }
}
