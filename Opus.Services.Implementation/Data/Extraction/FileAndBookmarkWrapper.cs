using CX.PdfLib.Services.Data;
using System.IO;
using Prism.Mvvm;
using System;

namespace Opus.Services.Implementation.Data.Extraction
{
    public class FileAndBookmarkWrapper : BindableBase, ILeveledItem
    {
        public Guid Id { get; }

        public int Level { get; set; }

        private int index;
        public int Index
        {
            get { return index; }
            set { SetProperty(ref index, value); }
        }

        private bool dropOpen;
        public bool DropOpen
        {
            get { return dropOpen; }
            set { SetProperty(ref dropOpen, value); }
        }

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
            Id = Guid.NewGuid();
        }

    }
}
