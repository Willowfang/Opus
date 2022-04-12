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
    public class ExtractSettingsDialog : DialogBase, IDialog, IDataErrorInfo
    {
        private bool emptyNameValid;

        private string? title;
        public string? Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        public string NameDescription { get; }
        public string NameHelper { get; }

        private bool alwaysAsk;
        public bool AlwaysAsk
        {
            get => alwaysAsk;
            set => SetProperty(ref alwaysAsk, value);
        }

        private bool pdfA;
        public bool PdfA
        {
            get => pdfA;
            set => SetProperty(ref pdfA, value);
        }

        private bool createZip;
        public bool CreateZip
        {
            get => createZip;
            set => SetProperty(ref createZip, value);
        }

        public bool PdfADisabled { get; set; }

        private int annotations;
        public int Annotations
        {
            get => annotations;
            set => SetProperty(ref annotations, value);
        }

        public bool IsAsking { get; }

        public ExtractSettingsDialog(string dialogTitle, bool isAsking = false,
            string? nameDescription = null, string? nameHelper = null, bool emptyNameValid = true)
            : base(dialogTitle) 
        {
            IsAsking = isAsking;
            NameDescription = nameDescription ?? Resources.Labels.Dialogs.ExtractionOptions.NameTemplate;
            NameHelper = nameHelper ?? Resources.Labels.Dialogs.ExtractionOptions.NameHelper;
            this.emptyNameValid = emptyNameValid;
        }

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
                    if (string.IsNullOrEmpty(Title) &&
                        emptyNameValid == false)
                    {
                        return Resources.Validation.General.NameEmpty;
                    }
                }

                return string.Empty;
            }
        }
    }
}
