using CX.PdfLib.Services.Data;
using System.IO;
using Prism.Mvvm;
using System;
using Opus.Services.Base;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CX.PdfLib.Extensions;

namespace Opus.Services.Implementation.Data.Extraction
{
    public class FileAndBookmarkWrapper : BindableBase, ILeveledItem, IIndexed
    {
        public Guid Id { get; }

        public int Level { get; set; }

        private int index;
        public int Index
        {
            get { return index; }
            set { SetProperty(ref index, value); }
        }

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set { SetProperty(ref isSelected, value); }
        }

        public string FileName { get; }
        public string FilePath { get; }
        public string Range
        {
            get
            {
                return Bookmark.Pages.Count < 1 ? "-" : Bookmark.StartPage + "-" + Bookmark.EndPage;
            }
        }
        public ILeveledBookmark Bookmark { get; }

        public FileAndBookmarkWrapper(ILeveledBookmark bookmark, string filePath, int index = 0, Guid id = default)
        {
            Level = 1;
            Bookmark = bookmark;
            FilePath = filePath;
            FileName = Path.GetFileNameWithoutExtension(filePath);
            Index = index;
            Id = id == Guid.Empty ? Guid.NewGuid() : id;
        }

        public FileAndBookmarkWrapper? FindParent(IList<FileAndBookmarkWrapper> storage)
        {
            return FindParent(storage, Bookmark.StartPage, Bookmark.EndPage);
        }
        public static FileAndBookmarkWrapper? FindParent(IList<FileAndBookmarkWrapper> storage, int childStartPage,
            int childEndPage)
        {
            if (storage is null)
                return null;

            return storage.LastOrDefault(x =>
                (x.Bookmark.StartPage < childStartPage && x.Bookmark.EndPage == childEndPage) ||
                (x.Bookmark.StartPage <= childStartPage && x.Bookmark.EndPage > childEndPage));
        }

        public FileAndBookmarkWrapper? FindPrecedingSibling(IList<FileAndBookmarkWrapper> storage,
            FileAndBookmarkWrapper? commonParent)
        {
            if (storage is null)
                return null;

            if (commonParent == null)
                return storage.LastOrDefault(x =>
                    x.Bookmark.Level == 1 &&
                    x.Bookmark.StartPage <= Bookmark.StartPage &&
                    x.Bookmark.EndPage < Bookmark.EndPage);

            return storage.LastOrDefault(x =>
                x.Bookmark.StartPage >= commonParent.Bookmark.StartPage &&
                x.Bookmark.EndPage <= commonParent.Bookmark.EndPage &&
                x.Bookmark.StartPage < Bookmark.StartPage);
        }

        public IList<FileAndBookmarkWrapper> FindChildren(IList<FileAndBookmarkWrapper> storage)
        {
            return storage.Where(x =>
                (x.Bookmark.StartPage > Bookmark.StartPage && x.Bookmark.EndPage == Bookmark.EndPage) ||
                (x.Bookmark.StartPage >= Bookmark.StartPage && x.Bookmark.EndPage < Bookmark.EndPage)).ToList();
        }

        public override bool Equals(object? obj) =>
            (obj is FileAndBookmarkWrapper other) && Equals(other);

        public bool Equals(FileAndBookmarkWrapper? other)
        {
            return CheckEquality(this, other);
        }

        public override int GetHashCode()
        {
            return Bookmark.Pages.GetHashCode();
        }

        public static bool operator ==(FileAndBookmarkWrapper? a, FileAndBookmarkWrapper? b)
        {
            return CheckEquality(a, b);
        }
        public static bool operator !=(FileAndBookmarkWrapper? a, FileAndBookmarkWrapper? b)
        {
            return !CheckEquality(a, b);
        }

        private static bool CheckEquality(FileAndBookmarkWrapper? a, FileAndBookmarkWrapper? b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Bookmark == b.Bookmark;
        }

    }
}
