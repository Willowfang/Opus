using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Core.Wrappers
{
    public class FileAndBookmarksStorage : BindableBase
    {
        public string FilePath { get; }
        public string FileName { get; }
        public ObservableCollection<BookmarkStorage> Bookmarks { get; set; }

        public FileAndBookmarksStorage(string filePath)
        {
            FilePath = filePath;
            FileName = Path.GetFileNameWithoutExtension(filePath);
            Bookmarks = new ObservableCollection<BookmarkStorage>();
        }
    }
}
