using Opus.Actions.Services.Redact;
using Opus.Common.Wrappers;
using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace Opus.Actions.Implementation.Redact
{
    /// <summary>
    /// Implementation for shared redaction properties
    /// </summary>
    public class RedactProperties : BindableBase, IRedactProperties
    {
        /// <summary>
        /// Files to redact.
        /// </summary>
        public ObservableCollection<FileStorage> Files { get; }

        private FileStorage? selectedFile;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public FileStorage? SelectedFile
        {
            get => selectedFile;
            set => SetProperty(ref selectedFile, value);
        }

        private string? wordsToRedact;
        /// <summary>
        /// Words to redact from files (may include wild cards * and ?), separated by comma.
        /// </summary>
        public string? WordsToRedact
        {
            get => wordsToRedact;
            set => SetProperty(ref wordsToRedact, value);
        }

        /// <summary>
        /// Create a new instance of redaction properties.
        /// </summary>
        public RedactProperties()
        {
            Files = new ObservableCollection<FileStorage>();
        }
    }
}
