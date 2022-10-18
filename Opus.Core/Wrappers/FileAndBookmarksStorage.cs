using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.IO;
using Opus.Services.Implementation.Data.Extraction;

namespace Opus.Core.Wrappers
{
    /// <summary>
    /// A class for storing information on bookmarks and a related file.
    /// </summary>
    public class FileAndBookmarksStorage : BindableBase
    {
        /// <summary>
        /// Path to the file containing the bookmarks.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Name of the file.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Bookmarks in the file.
        /// </summary>
        public ObservableCollection<FileAndBookmarkWrapper> Bookmarks { get; set; }

        /// <summary>
        /// Create a new storage class for bookmarks and their containing file.
        /// </summary>
        /// <param name="filePath"></param>
        public FileAndBookmarksStorage(string filePath)
        {
            FilePath = filePath;
            FileName = Path.GetFileNameWithoutExtension(filePath);
            Bookmarks = new ObservableCollection<FileAndBookmarkWrapper>();
        }
    }
}
