using Opus.Actions.Services.Base;
using Opus.Common.Wrappers;
using System.Collections.ObjectModel;

namespace Opus.Actions.Services.Extract
{
    /// <summary>
    /// Properties for extraction action.
    /// </summary>
    public interface IExtractionActionProperties : IActionProperties
    {
        /// <summary>
        /// Store last known file path as the starting point for user's directory destination selection.
        /// </summary>
        public string? CurrentFilePath { get; set; }

        /// <summary>
        /// Collection for storing info on files, whose bookmarks can be selected and extracted.
        /// </summary>
        public ObservableCollection<FileAndBookmarksStorage> Files { get; }

        /// <summary>
        /// The file that is being currently worked on. Notify properties on change.
        /// </summary>
        public FileAndBookmarksStorage? SelectedFile { get; set; }

        /// <summary>
        /// The collection of bookmarks from the currently selected file (or null, if no file selected 
        /// or no bookmarks).
        /// </summary>
        public ObservableCollection<FileAndBookmarkWrapper>? FileBookmarks { get; }

        /// <summary>
        /// The bookmark that has been last selected by user.
        /// </summary>
        public FileAndBookmarkWrapper? SelectedBookmark { get; set; }
    }
}
