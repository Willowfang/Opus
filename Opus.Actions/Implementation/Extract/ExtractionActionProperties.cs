using Opus.Actions.Services.Extract;
using Opus.Common.Wrappers;
using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace Opus.Actions.Implementation.Extract
{
    /// <summary>
    /// Implementation for extraction action properties.
    /// </summary>
    public class ExtractionActionProperties : BindableBase, IExtractionActionProperties
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string? CurrentFilePath { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ObservableCollection<FileAndBookmarksStorage> Files { get; private set; }

        private FileAndBookmarksStorage? selectedFile;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public FileAndBookmarksStorage? SelectedFile
        {
            get => selectedFile;
            set
            {
                SetProperty(ref selectedFile, value);
                RaisePropertyChanged(nameof(FileBookmarks));
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ObservableCollection<FileAndBookmarkWrapper>? FileBookmarks
        {
            get =>
                SelectedFile != null && SelectedFile.Bookmarks != null
                    ? SelectedFile.Bookmarks
                    : null;
        }

        private FileAndBookmarkWrapper? selectedBookmark;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public FileAndBookmarkWrapper? SelectedBookmark
        {
            get { return selectedBookmark; }
            set { SetProperty(ref selectedBookmark, value); }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ExtractionActionProperties()
        {
            Files = new ObservableCollection<FileAndBookmarksStorage>();
        }
    }
}
