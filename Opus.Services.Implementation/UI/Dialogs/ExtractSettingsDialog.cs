using CX.LoggingLib;
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
        private string? title;
        public string? Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

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

        public bool PdfADisabled { get; set; }

        private int annotations;
        public int Annotations
        {
            get => annotations;
            set => SetProperty(ref annotations, value);
        }

        public bool IsAsking { get; }

        public ExtractSettingsDialog(string dialogTitle, bool isAsking = false)
            : base(dialogTitle) 
        {
            IsAsking = isAsking;
        }
    }
}
