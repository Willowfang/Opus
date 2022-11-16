using Opus.Actions.Services.WorkCopy;
using Opus.Common.Wrappers;
using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace Opus.Actions.Implementation.WorkCopy
{
    /// <summary>
    /// Implementation for <see cref="IWorkCopyProperties"/>.
    /// </summary>
    public class WorkCopyProperties : BindableBase, IWorkCopyProperties
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ObservableCollection<FileStorage> OriginalFiles { get; set; }

        private FileStorage? selectedFile;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public FileStorage? SelectedFile
        {
            get => selectedFile;
            set => SetProperty(ref selectedFile, value);
        }

        /// <summary>
        /// Create new implementation instance.
        /// </summary>
        public WorkCopyProperties()
        {
            // Initialize collection.

            OriginalFiles = new ObservableCollection<FileStorage>();
        }
    }
}
