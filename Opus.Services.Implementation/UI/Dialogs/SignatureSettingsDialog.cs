using Opus.Services.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.UI.Dialogs
{
    public class SignatureSettingsDialog : DialogBase, IDialog, IDataErrorInfo
    {
        private string? suffix;
        public string? Suffix
        {
            get => suffix;
            set
            {
                SetProperty(ref suffix, value);
            }
        }

        public string? Error
        {
            get => null;
        }

        public SignatureSettingsDialog(string dialogTitle)
            : base(dialogTitle) { }

        public string this[string propertyName]
        {
            get
            {
                if (propertyName == nameof(Suffix))
                {
                    if (string.IsNullOrEmpty(suffix))
                    {
                        return Resources.Validation.General.NameEmpty;
                    }
                }

                return string.Empty;
            }
        }
    }
}
