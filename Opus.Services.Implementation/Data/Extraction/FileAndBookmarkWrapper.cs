using CX.PdfLib.Services.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.Data.Extraction
{
    public class FileAndBookmarkWrapper : ILeveledItem
    {
        public int Level { get; set; }
        public int Index { get; set; }
        public string FileName { get; }
        public string FilePath { get; }
        public ILeveledBookmark Bookmark { get; }

        public FileAndBookmarkWrapper(ILeveledBookmark bookmark, string filePath, int index = 0)
        {
            Level = 1;
            Bookmark = bookmark;
            FilePath = filePath;
            FileName = Path.GetFileNameWithoutExtension(filePath);
            Index = index;
        }

    }
}
