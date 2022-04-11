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
    public class WorkCopySettingsDialog : DialogBase, IDialog, IDataErrorInfo
    {
        private string? titleTemplate;
        public string? TitleTemplate
        {
            get => titleTemplate;
            set
            {
                SetProperty(ref titleTemplate, value);
            }
        }

        private bool flattenRedactions;
        public bool FlattenRedactions
        {
            get => flattenRedactions;
            set => SetProperty(ref flattenRedactions, value);
        }

        public string? Error
        {
            get => null;
        }

        public WorkCopySettingsDialog(string dialogTitle)
            : base(dialogTitle) { }

        public string this[string propertyName]
        {
            get
            {
                if (propertyName == nameof(TitleTemplate))
                {
                    if (string.IsNullOrEmpty(titleTemplate))
                    {
                        return Resources.Validation.General.NameEmpty;
                    }
                }

                return string.Empty;
            }
        }
    }
}
