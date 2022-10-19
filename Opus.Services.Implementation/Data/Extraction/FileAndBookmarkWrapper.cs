using CX.PdfLib.Services.Data;
using System.IO;
using Prism.Mvvm;
using System;
using Opus.Services.Base;
using System.Collections.Generic;
using System.Linq;

namespace Opus.Services.Implementation.Data.Extraction
{
    /// <summary>
    /// Wrapper for wrapping bookmark and related file info.
    /// </summary>
    public class FileAndBookmarkWrapper : BindableBase, ILeveledItem, IIndexed
    {
        /// <summary>
        /// Guid for identifying particular wrapper.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Level of this wrapper in the hierarchy.
        /// </summary>
        public int Level { get; set; }

        private int index;

        /// <summary>
        /// Index of this wrapper in bookmark sequence.
        /// </summary>
        public int Index
        {
            get { return index; }
            set { SetProperty(ref index, value); }
        }

        private bool isSelected;

        /// <summary>
        /// If true, this wrapper has been selected.
        /// </summary>
        public bool IsSelected
        {
            get => isSelected;
            set { SetProperty(ref isSelected, value); }
        }

        /// <summary>
        /// Name of the file associated with the bookmark.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Path of the file associated with the bookmark.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Page range of the bookmark as a string.
        /// </summary>
        public string Range
        {
            get
            {
                return Bookmark.Pages.Count < 1 ? "-" : Bookmark.StartPage + "-" + Bookmark.EndPage;
            }
        }

        /// <summary>
        /// Bookmark contained in this wrapper.
        /// </summary>
        public ILeveledBookmark Bookmark { get; }

        /// <summary>
        /// Create a new wrapper.
        /// </summary>
        /// <param name="bookmark">Bookmark to include in this wrapper.</param>
        /// <param name="filePath">Filepath of the related file.</param>
        /// <param name="index">Index in the bookmark sequence.</param>
        /// <param name="id">Id associated with this combo (will be created if not given).</param>
        public FileAndBookmarkWrapper(
            ILeveledBookmark bookmark,
            string filePath,
            int index = 0,
            Guid id = default
        )
        {
            Level = 1;
            Bookmark = bookmark;
            FilePath = filePath;
            FileName = Path.GetFileNameWithoutExtension(filePath);
            Index = index;
            Id = id == Guid.Empty ? Guid.NewGuid() : id;
        }

        /// <summary>
        /// Find the parent of the bookmark contained in this wrapper, if there is one.
        /// </summary>
        /// <param name="storage">List to look the parent from.</param>
        /// <returns>The parent or null, if no parent found.</returns>
        public FileAndBookmarkWrapper? FindParent(IList<FileAndBookmarkWrapper> storage)
        {
            return FindParent(storage, Bookmark.StartPage, Bookmark.EndPage);
        }

        /// <summary>
        /// Find the parent of a bookmark, if there is one.
        /// </summary>
        /// <param name="storage">List to look the parent from.</param>
        /// <param name="childStartPage">Start page of the child.</param>
        /// <param name="childEndPage">End page of the child.</param>
        /// <returns>Parent of null, if no parent is found.</returns>
        public static FileAndBookmarkWrapper? FindParent(
            IList<FileAndBookmarkWrapper> storage,
            int childStartPage,
            int childEndPage
        )
        {
            if (storage is null)
                return null;

            return storage.LastOrDefault(
                x =>
                    (x.Bookmark.StartPage < childStartPage && x.Bookmark.EndPage == childEndPage)
                    || (x.Bookmark.StartPage <= childStartPage && x.Bookmark.EndPage > childEndPage)
            );
        }

        /// <summary>
        /// Find the sibling of a given bookmark that is the nearest "elder" sibling.
        /// </summary>
        /// <param name="storage">List to look the sibling from.</param>
        /// <param name="commonParent">Parent wrapper common to this wrappers bookmark and the sibling.e2</param>
        /// <returns>Sibling or null, if no sibling is found.</returns>
        public FileAndBookmarkWrapper? FindPrecedingSibling(
            IList<FileAndBookmarkWrapper> storage,
            FileAndBookmarkWrapper? commonParent
        )
        {
            if (storage is null)
                return null;

            if (commonParent == null)
                return storage.LastOrDefault(
                    x =>
                        x.Bookmark.Level == 1
                        && x.Bookmark.StartPage <= Bookmark.StartPage
                        && x.Bookmark.EndPage < Bookmark.EndPage
                );

            return storage.LastOrDefault(
                x =>
                    x.Bookmark.StartPage >= commonParent.Bookmark.StartPage
                    && x.Bookmark.EndPage <= commonParent.Bookmark.EndPage
                    && x.Bookmark.StartPage < Bookmark.StartPage
            );
        }

        /// <summary>
        /// Find the children of the bookmark contained in this wrapper.
        /// </summary>
        /// <param name="storage">List of the bookmarks to search for children.</param>
        /// <returns>Children, if some were found.</returns>
        public IList<FileAndBookmarkWrapper> FindChildren(IList<FileAndBookmarkWrapper> storage)
        {
            return storage
                .Where(
                    x =>
                        (
                            x.Bookmark.StartPage > Bookmark.StartPage
                            && x.Bookmark.EndPage == Bookmark.EndPage
                        )
                        || (
                            x.Bookmark.StartPage >= Bookmark.StartPage
                            && x.Bookmark.EndPage < Bookmark.EndPage
                        )
                )
                .ToList();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj) =>
            (obj is FileAndBookmarkWrapper other) && Equals(other);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(FileAndBookmarkWrapper? other)
        {
            return CheckEquality(this, other);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Bookmark.Pages.GetHashCode();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(FileAndBookmarkWrapper? a, FileAndBookmarkWrapper? b)
        {
            return CheckEquality(a, b);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(FileAndBookmarkWrapper? a, FileAndBookmarkWrapper? b)
        {
            return !CheckEquality(a, b);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static bool CheckEquality(FileAndBookmarkWrapper? a, FileAndBookmarkWrapper? b)
        {
            if (a is null && b is null)
                return true;
            if (a is null || b is null)
                return false;
            return a.Bookmark == b.Bookmark;
        }
    }
}
