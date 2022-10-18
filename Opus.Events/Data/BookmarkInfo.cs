using System;

namespace Opus.Events.Data
{
    /// <summary>
    /// Information on a bookmark as an event payload.
    /// </summary>
    public class BookmarkInfo
    {
        /// <summary>
        /// Page on which the bookmark begins.
        /// </summary>
        public int StartPage { get; }

        /// <summary>
        /// Page to which the bookmark ends.
        /// </summary>
        public int EndPage { get; }

        /// <summary>
        /// Name of the bookmark in the bookmark tree.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Path to the file wherein the bookmark is contained.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// A guid to distinguish the bookmark from others.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Create a new bookmark info payload.
        /// </summary>
        /// <param name="startPage">Page on which the bookmark resides.</param>
        /// <param name="endPage">The last page contained in the bookmark.</param>
        /// <param name="title">Name of the bookmarks shown in the bookmark tree.</param>
        /// <param name="filePath">Filepath of the file of which the bookmark is a part of.</param>
        /// <param name="id">Guid to find and identify this bookmark.</param>
        public BookmarkInfo(
            int startPage,
            int endPage,
            string title,
            string filePath,
            Guid id = default
        )
        {
            StartPage = startPage;
            EndPage = endPage;
            Title = title;
            FilePath = filePath;
            Id = id;
        }
    }
}
