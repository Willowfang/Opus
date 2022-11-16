using Opus.Services.UI;
using System.ComponentModel;

namespace Opus.Services.Implementation.UI.Dialogs
{
    /// <summary>
    /// A dialog for choosing settings applied to extractions.
    /// </summary>
    public class ExtractSettingsDialog : DialogBase, IDialog, IDataErrorInfo
    {
        private bool emptyNameValid;

        private string? title;

        /// <summary>
        /// Title of the dialog.
        /// </summary>
        public string? Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        /// <summary>
        /// Description for the name field.
        /// </summary>
        public string NameDescription { get; }

        /// <summary>
        /// Helper text for the name field.
        /// </summary>
        public string NameHelper { get; }

        private bool alwaysAsk;

        /// <summary>
        /// If true, name template will be inquired every time.
        /// </summary>
        public bool AlwaysAsk
        {
            get => alwaysAsk;
            set => SetProperty(ref alwaysAsk, value);
        }

        private bool pdfA;

        /// <summary>
        /// If true, documents will be converted to pdf/a.
        /// </summary>
        public bool PdfA
        {
            get => pdfA;
            set => SetProperty(ref pdfA, value);
        }

        private bool createZip;

        /// <summary>
        /// If true, documents will be compressed into a zip-file.
        /// </summary>
        public bool CreateZip
        {
            get => createZip;
            set => SetProperty(ref createZip, value);
        }

        /// <summary>
        /// If true, pdf/a cannot be produced.
        /// </summary>
        public bool PdfADisabled { get; set; }

        private bool groupByFiles;

        /// <summary>
        /// If true, bookmarks will be grouped by files when producing a single output file.
        /// </summary>
        public bool GroupByFiles
        {
            get => groupByFiles;
            set => SetProperty(ref groupByFiles, value);
        }

        private int annotations;

        /// <summary>
        /// What to do with annotations.
        /// </summary>
        public int Annotations
        {
            get => annotations;
            set => SetProperty(ref annotations, value);
        }

        private bool openAfterComplete;

        /// <summary>
        /// Open the destination file or folder after extraction is complete.
        /// </summary>
        public bool OpenAfterComplete
        {
            get => openAfterComplete;
            set => SetProperty(ref openAfterComplete, value);
        }

        /// <summary>
        /// If true, the dialog is currently being used while processing files (as opposed to choosing the
        /// settings beforehand).
        /// </summary>
        public bool IsAsking { get; }

        /// <summary>
        /// Create a new dialog for choosing extraction settings.
        /// </summary>
        /// <param name="dialogTitle">Title for the dialog.</param>
        /// <param name="isAsking">Whether the dialog is asking for the name template while processing files.</param>
        /// <param name="nameDescription">Description for the name field.</param>
        /// <param name="nameHelper">Helper text for the name field.</param>
        /// <param name="emptyNameValid">If true, an empty name template is allowed.</param>
        public ExtractSettingsDialog(
            string dialogTitle,
            bool isAsking = false,
            string? nameDescription = null,
            string? nameHelper = null,
            bool emptyNameValid = true
        ) : base(dialogTitle)
        {
            IsAsking = isAsking;
            NameDescription =
                nameDescription ?? Resources.Labels.Dialogs.ExtractionOptions.NameTemplate;
            NameHelper = nameHelper ?? Resources.Labels.Dialogs.ExtractionOptions.NameHelper;
            this.emptyNameValid = emptyNameValid;
        }

        /// <summary>
        /// Validation error. Always returns null.
        /// </summary>
        public string? Error
        {
            get => null;
        }

        /// <summary>
        /// Validation.
        /// </summary>
        /// <param name="propertyName">Property to validate.</param>
        /// <returns></returns>
        public string this[string propertyName]
        {
            get
            {
                if (propertyName == nameof(Title))
                {
                    if (string.IsNullOrEmpty(Title) && emptyNameValid == false)
                    {
                        return Resources.Validation.General.NameEmpty;
                    }
                }

                return string.Empty;
            }
        }
    }
}
