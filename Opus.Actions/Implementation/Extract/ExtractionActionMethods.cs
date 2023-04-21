using Opus.Actions.Services.Extract;
using Opus.Events.Data;
using Opus.Common.Wrappers;
using Opus.Common.Logging;
using Opus.Common.Dialogs;
using Opus.Common.Services.Dialogs;
using System.Collections.ObjectModel;
using WF.LoggingLib;
using WF.PdfLib.Common;
using Opus.Events;
using Prism.Events;
using System.Windows.Controls;
using WF.PdfLib.Services.Data;
using WF.PdfLib.Services;
using System.Windows;

namespace Opus.Actions.Implementation.Extract
{
    /// <summary>
    /// Methods for extraction actions.
    /// </summary>
    public class ExtractionActionMethods : LoggingCapable<ExtractionActionMethods>, IExtractionActionMethods
    {
        private readonly IExtractionActionProperties properties;
        private readonly IDialogAssist dialogAssist;
        private readonly IEventAggregator eventAggregator;
        private readonly IBookmarkService bookmarkService;

        /// <summary>
        /// Create new extraction methods instance.
        /// </summary>
        /// <param name="dialogAssist">Service for showing dialogs.</param>
        /// <param name="properties">Extraction action properties service.</param>
        /// <param name="eventAggregator">Event service.</param>
        /// <param name="bookmarkService">Utility service for manipulating bookmarks.</param>
        /// <param name="logbook">Logging service.</param>
        public ExtractionActionMethods(
            IExtractionActionProperties properties,
            IDialogAssist dialogAssist,
            IEventAggregator eventAggregator,
            IBookmarkService bookmarkService,
            ILogbook logbook) : base(logbook)
        {
            this.properties = properties;
            this.dialogAssist = dialogAssist;
            this.eventAggregator = eventAggregator;
            this.bookmarkService = bookmarkService;
        }

        #region Add new files
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public async Task AddNewFiles(string[] filePaths) => await AddNewFilesWithProperties(filePaths, properties);

        private async Task AddNewFilesWithProperties(string[] filePaths, IExtractionActionProperties properties)
        {
            logbook.Write($"Adding new files.", LogLevel.Information);

            foreach (string path in filePaths)
            {
                // Only add files if they are new

                if (properties.Files.Any(f => f.FilePath == path) == false)
                {
                    await AddNewFilesStorageToFiles(path, properties);
                }
            }

            AddNewFilesSelectFile(filePaths, properties);

            // Store the last filepath for future reference
            properties.CurrentFilePath = filePaths.LastOrDefault();

            logbook.Write($"New files added.", LogLevel.Information);
        }

        private async Task AddNewFilesStorageToFiles(string path, IExtractionActionProperties properties)
        {
            logbook.Write($"Creating a file storage.", LogLevel.Debug);

            FileAndBookmarksStorage storage = new FileAndBookmarksStorage(path);

            // Find bookmarks for each file and store them in a wrapper along with file information

            foreach (ILeveledBookmark found in await bookmarkService.FindBookmarks(path))
            {
                storage.Bookmarks.Add(new FileAndBookmarkWrapper(found, path));
            }

            properties.Files.Add(storage);

            logbook.Write($"File storage created and added.", LogLevel.Debug);
        }

        private void AddNewFilesSelectFile(string[] filePaths, IExtractionActionProperties properties)
        {
            // If only one file was added, select it. Otherwise keep selection.

            if (filePaths.Count() == 1)
            {
                properties.SelectedFile = properties.Files.Last();
            }
            else if (properties.SelectedFile == null)
            {
                properties.SelectedFile = properties.Files.FirstOrDefault();
            }
        }
        #endregion

        #region Deselect bookmark
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="id"></param>
        public void DeselectBookmark(Guid id) => DeselectBookmarksWithProperties(id, properties);

        private void DeselectBookmarksWithProperties(Guid id, IExtractionActionProperties properties)
        {
            logbook.Write($"Deselecting bookmarks.", LogLevel.Debug);

            // Search for the correct bookmark in all files

            foreach (FileAndBookmarksStorage file in properties.Files)
            {
                foreach (FileAndBookmarkWrapper bookmark in file.Bookmarks)
                {
                    // If bookmark is found, deselect it, any children it has and
                    // its closest parent (children are included in its page range
                    // and parent has to be shown for hierarchical display purposes)

                    if (bookmark.Id == id)
                    {
                        DeSelectParent(bookmark, file.Bookmarks);
                        bookmark.IsSelected = false;
                        DeSelectChildrenRecursively(bookmark, file.Bookmarks);

                        logbook.Write($"Bookmarks deselected.", LogLevel.Debug);

                        return;
                    }
                }
            }

            foreach (FileAndBookmarksStorage file in properties.WholeFilesSelected)
            {
                if (file.FileAsBookmark == null) continue;

                if (file.FileAsBookmark.Id == id)
                {
                    foreach (FileAndBookmarkWrapper wrapper in file.Bookmarks)
                    {
                        wrapper.IsSelected = false;
                    }

                    file.FileAsBookmark = null;

                    properties.Files.Add(file);

                    properties.WholeFilesSelected.Remove(file);

                    logbook.Write($"Bookmarks deselected.", LogLevel.Debug);

                    return;
                }
            }
        }
        #endregion

        #region Common private
        /// <summary>
        /// Method for handling new user requested bookmarks.
        /// <para>
        /// Bookmark will be added to the hierarchy of current documents bookmarks and immediately selected.
        /// </para>
        /// </summary>
        /// <param name="input">Info for the new bookmark.</param>
        /// <param name="properties">Extraction properties.</param>
        private void BookmarkAdded(BookmarkInfo input, IExtractionActionProperties properties)
        {
            // Null checks

            if (properties.FileBookmarks == null)
            {
                logbook.Write($"Collection of file and bookmark info was null.", LogLevel.Error);

                throw new ArgumentNullException(nameof(properties.FileBookmarks));
            }

            if (properties.SelectedFile == null)
            {
                logbook.Write($"No file was selected.", LogLevel.Error);

                throw new ArgumentNullException(nameof(properties.SelectedFile));
            }

            logbook.Write($"Adding bookmark to collection.", LogLevel.Debug);

            // Sort the bookmarks in order in a new list. Find the parent of the newly added bookmark,
            // if it has a parent.

            List<FileAndBookmarkWrapper> sorted = properties.FileBookmarks
                .OrderBy(x => x.Bookmark.StartPage)
                .ToList();

            FileAndBookmarkWrapper? parent = FileAndBookmarkWrapper.FindParent(
                sorted,
                input.StartPage,
                input.EndPage
            );

            FileAndBookmarkWrapper addMark = BookmarkAddedGetNewMark(input, parent);

            // Find out if the new bookmark is the first sibling or if there are siblings higher up in the
            // bookmarks tree. Also, find out, if the new bookmark will have children (in order to adjust
            // their levels).

            FileAndBookmarkWrapper? precedingSibling = addMark.FindPrecedingSibling(sorted, parent);

            IList<FileAndBookmarkWrapper> children = addMark.FindChildren(sorted);

            BookmarkAddedInsertBookmark(addMark, parent, precedingSibling, properties.FileBookmarks);

            // Adjust childrens' info, if there are any.

            BookmarkAddedAdjustChildren(children, properties.FileBookmarks, properties.SelectedFile);

            logbook.Write($"Bookmark added.", LogLevel.Debug);
        }

        private void BookmarkAddedAdjustChildren(
            IList<FileAndBookmarkWrapper> children,
            ObservableCollection<FileAndBookmarkWrapper> fileBookmarks,
            FileAndBookmarksStorage selectedFile)
        {
            logbook.Write($"Adjusting bookmark children.", LogLevel.Debug);

            foreach (FileAndBookmarkWrapper child in children)
            {
                int childIndex = fileBookmarks.IndexOf(child);

                fileBookmarks.RemoveAt(childIndex);

                LeveledBookmark internalChildMark = new LeveledBookmark(
                    child.Bookmark.Level + 1,
                    child.Bookmark.Title,
                    child.Bookmark.Pages,
                    child.Bookmark.Children);

                FileAndBookmarkWrapper childWrapper = new FileAndBookmarkWrapper(
                    internalChildMark,
                    selectedFile.FilePath);

                fileBookmarks.Insert(childIndex, childWrapper);
            }

            logbook.Write($"Children adjusted.", LogLevel.Debug);
        }

        private FileAndBookmarkWrapper BookmarkAddedGetNewMark(
            BookmarkInfo input,
            FileAndBookmarkWrapper? parent)
        {
            int level = parent == null ? 1 : parent.Bookmark.Level + 1;

            LeveledBookmark internalMark = new LeveledBookmark(
                level,
                input.Title,
                input.StartPage,
                input.EndPage - input.StartPage + 1);

            return new FileAndBookmarkWrapper(
                internalMark,
                input.FilePath
            );
        }

        private void BookmarkAddedInsertBookmark(
            FileAndBookmarkWrapper addMark,
            FileAndBookmarkWrapper? parent,
            FileAndBookmarkWrapper? precedingSibling,
            ObservableCollection<FileAndBookmarkWrapper> fileBookmarks)
        {
            logbook.Write($"Inserting bookmark to collection.", LogLevel.Debug);
            // Default to first bookmark.

            int index = 0;

            // Sub-level bookmark, but first of its kind.

            if (parent != null && precedingSibling == null)
                index = fileBookmarks.IndexOf(parent) + 1;

            // Not the first of its kind, top-level or sub-level

            if (precedingSibling != null)
                index = fileBookmarks.IndexOf(precedingSibling) + 1;

            // Add bookmark

            addMark.IsSelected = true;

            fileBookmarks.Insert(index, addMark);

            logbook.Write($"Bookmark inserted.", LogLevel.Debug);
        }

        /// <summary>
        /// Select all children of a bookmark in a recursive manner.
        /// </summary>
        /// <param name="mark">Bookmark whose children should be selected</param>
        /// <param name="properties">Extraction properties.</param>
        private void SelectChildrenRecursively(
            FileAndBookmarkWrapper mark,
            IExtractionActionProperties properties)
        {
            // If there are no bookmarks, do not search for children.

            if (properties.FileBookmarks == null) return;

            foreach (FileAndBookmarkWrapper child in mark.FindChildren(properties.FileBookmarks))
            {
                child.IsSelected = true;
                SelectChildrenRecursively(child, properties);
            }
        }

        /// <summary>
        /// Deselect all children of a bookmark in a recursive manner.
        /// </summary>
        /// <param name="mark">Bookmark whose children should be deselected</param>
        /// <param name="bookmarks">Bookmarks to look the children in</param>
        private void DeSelectChildrenRecursively(
            FileAndBookmarkWrapper mark,
            ObservableCollection<FileAndBookmarkWrapper> bookmarks
        )
        {
            if (mark is null || bookmarks is null)
                return;

            IList<FileAndBookmarkWrapper> children = mark.FindChildren(bookmarks);
            if (children.All(c => c.IsSelected))
            {
                foreach (FileAndBookmarkWrapper child in mark.FindChildren(bookmarks))
                {
                    child.IsSelected = false;
                    DeSelectChildrenRecursively(child, bookmarks);
                }
            }
        }

        /// <summary>
        /// Deselect the parent of a bookmark.
        /// </summary>
        /// <param name="mark">Bookmark whose parent should be deselected.</param>
        /// <param name="bookmarks">Bookmarks to look the parent in.</param>
        private void DeSelectParent(
            FileAndBookmarkWrapper mark,
            ObservableCollection<FileAndBookmarkWrapper> bookmarks)
        {
            FileAndBookmarkWrapper? parent = mark.FindParent(bookmarks);
            if (parent != null)
            {
                parent.IsSelected = false;
            }
        }
        #endregion

        #region Execute add
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteAdd()
        {
            if (properties.SelectedFile == null)
            {
                logbook.Write($"Selected file was null.", LogLevel.Warning);

                return;
            }

            logbook.Write($"Adding new bookmark.", LogLevel.Information);

            // Create a dialog for user-provided options and show said dialog.

            BookmarkDialog dialog = await ShowAddDialog();

            // The user canceled, just return.

            if (dialog.IsCanceled)
            {
                logbook.Write($"Bookmark addition cancelled.", LogLevel.Information);

                return;
            }

            // The user confirmed, create new info for adding a bookmark and call appropriate method.

            BookmarkInfo info = AddCreateBookmarkInfo(dialog, properties.SelectedFile.FilePath);

            BookmarkAdded(info, properties);

            logbook.Write($"Bookmark added.", LogLevel.Information);
        }

        private async Task<BookmarkDialog> ShowAddDialog()
        {
            BookmarkDialog dialog = new BookmarkDialog(Resources.Labels.Dialogs.Bookmark.New)
            {
                StartPage = 1,
                EndPage = 1
            };

            await dialogAssist.Show(dialog);

            return dialog;
        }

        private BookmarkInfo AddCreateBookmarkInfo(BookmarkDialog dialog, string filePath)
        {
            return new BookmarkInfo(
                dialog.StartPage,
                dialog.EndPage,
                dialog.Title,
                filePath,
                null
            );
        }
        #endregion

        #region Execute selection
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void ExecuteSelection(SelectionChangedEventArgs parameter)
        {
            if (parameter.AddedItems.Count > 0)
            {
                // Select the children of the selected bookmark (because they are included in the
                // page range of the parent).

                // All selected bookmarks will be hidden.

                FileAndBookmarkWrapper? wrapper = parameter.AddedItems[0] as FileAndBookmarkWrapper;

                if (wrapper == null) return;

                SelectChildrenRecursively(wrapper, properties);

                BookmarkInfo info = SelectCreateBookmarkInfo(wrapper);

                // Send info of the selected bookmark to Extraction Order ViewModel through events.

                eventAggregator.GetEvent<BookmarkSelectedEvent>().Publish(info);
            }
        }

        private BookmarkInfo SelectCreateBookmarkInfo(FileAndBookmarkWrapper wrapper)
        {
            return new BookmarkInfo(
                wrapper.Bookmark.StartPage,
                wrapper.Bookmark.EndPage,
                wrapper.Bookmark.Title,
                wrapper.FilePath,
                wrapper.Bookmark.Children,
                wrapper.Id);
        }
        #endregion

        #region Execute file view
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void ExecuteViewFile()
        {
            // Null check.

            if (properties.SelectedFile == null) return;

            logbook.Write($"Viewing selected file.", LogLevel.Information);

            // Open the file in system default program

            new System.Diagnostics.Process()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo(
                    properties.SelectedFile.FilePath)
                {
                    UseShellExecute = true
                }
            }.Start();


            logbook.Write($"File at {properties.SelectedFile.FilePath} opened externally.", LogLevel.Information);
        }
        #endregion

        #region Execute delete
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void ExecuteDeleteFile()
        {
            // Null check.

            if (properties.SelectedFile == null) return;

            logbook.Write($"Deleting file.", LogLevel.Information);

            // Remove the selected file from the collection

            int index = properties.Files.IndexOf(properties.SelectedFile);

            string path = properties.SelectedFile.FilePath;

            properties.Files.Remove(properties.SelectedFile);

            // Publish an event to inform a file has been removed (Extract Order ViewModel needs said info
            // to remove selected bookmarks associated with given file).

            eventAggregator.GetEvent<BookmarkFileDeletedEvent>().Publish(path);

            // Select another file based on how many files there are left

            DeleteFileSelectNext(properties, index);

            logbook.Write($"File deleted.", LogLevel.Information);
        }

        private void DeleteFileSelectNext(
            IExtractionActionProperties properties,
            int deletedIndex)
        {
            if (properties.Files.Count > 0)
            {
                if (deletedIndex == 0 || properties.Files.Count == 1)
                {
                    properties.SelectedFile = properties.Files[0];
                }
                else
                {
                    properties.SelectedFile = properties.Files[deletedIndex - 1];
                }
            }
            else
            {
                properties.SelectedFile = null;
            }
        }
        #endregion

        #region Execute select whole file
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void ExecuteSelectWholeFile()
        {
            if (properties.SelectedFile == null)
                return;

            ILeveledBookmark fileAsBookmark =
                bookmarkService.DocumentAsBookmark(properties.SelectedFile.FilePath);

            FileAndBookmarkWrapper fileAsBookmarkWrapper =
                new FileAndBookmarkWrapper(fileAsBookmark, properties.SelectedFile.FilePath);

            int selectedIndex = properties.Files.IndexOf(properties.SelectedFile);

            properties.SelectedFile.FileAsBookmark = fileAsBookmarkWrapper;

            properties.WholeFilesSelected.Add(properties.SelectedFile);

            if (properties.Files.Count > 1)
            {
                if (selectedIndex == 0)
                {
                    properties.SelectedFile = properties.Files[1];
                }
                else
                {
                    properties.SelectedFile = properties.Files[selectedIndex - 1];
                }
            }
            else
            {
                properties.SelectedFile = null;
            }

            properties.Files.RemoveAt(selectedIndex);

            BookmarkInfo info = SelectCreateBookmarkInfo(fileAsBookmarkWrapper);

            eventAggregator.GetEvent<BookmarkSelectedEvent>().Publish(info);
        }
        #endregion
    }
}
