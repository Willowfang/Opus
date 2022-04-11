using CX.LoggingLib;
using Opus.Services.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.UI.Dialogs
{
    public class FileTitleDialog : DialogBase, IDialog, IDataErrorInfo
    {
        private string? title;
        public string? Title
        {
            get => title;
            set
            {
                SetProperty(ref title, value);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public FileTitleDialog(string dialogTitle)
            : base(dialogTitle) { }

        public string? Error
        {
            get => null;
        }

        public string this[string propertyName]
        {
            get
            {
                if (propertyName == nameof(Title))
                {
                    if (string.IsNullOrWhiteSpace(Title))
                    {
                        return Resources.Validation.General.NameEmpty;
                    }
                }

                return string.Empty;
            }
        }
    }
}
