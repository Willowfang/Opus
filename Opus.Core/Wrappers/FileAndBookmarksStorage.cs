using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.IO;
using Opus.Services.Implementation.Data.Extraction;

namespace Opus.Core.Wrappers
{
    public class FileAndBookmarksStorage : BindableBase
    {
        public string FilePath { get; }
        public string FileName { get; }
        public ObservableCollection<FileAndBookmarkWrapper> Bookmarks { get; set; }

        public FileAndBookmarksStorage(string filePath)
        {
            FilePath = filePath;
            FileName = Path.GetFileNameWithoutExtension(filePath);
            Bookmarks = new ObservableCollection<FileAndBookmarkWrapper>();
        }
    }
}
