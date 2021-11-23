using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Core.Dialog
{
    public class ProgressDialog : BindableBase, IDialog
    {
        private int percent;
        public int Percent
        {
            get => percent;
            set => SetProperty(ref percent, value);
        }
        private string phase;
        public string Phase
        {
            get => phase;
            set => SetProperty(ref phase, value);
        }
        private string item;
        public string Item
        {
            get => item;
            set => SetProperty(ref item, value);
        }

        public ProgressDialog(int percent = 0, string phase = null, string item = null)
        {
            Percent = percent;
            Phase = phase;
            Item = item;
        }
    }
}
