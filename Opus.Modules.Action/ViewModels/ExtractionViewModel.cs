using System.Collections.ObjectModel;
using Prism.Events;
using Prism.Commands;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Controls;
using Opus.Core.Base;
using WF.PdfLib.Services;
using WF.PdfLib.Services.Data;
using WF.PdfLib.Common;
using Opus.Core.Wrappers;
using Opus.Events.Data;
using System.Threading.Tasks;
using AsyncAwaitBestPractices.MVVM;
using Opus.Values;
using Opus.Services.Input;
using Opus.Events;
using Opus.Services.UI;
using Opus.Services.Implementation.UI.Dialogs;
using Opus.Core.Executors;
using WF.LoggingLib;
using Opus.Services.Configuration;
using Opus.Services.Implementation.Data.Extraction;
using System;

namespace Opus.Modules.Action.ViewModels
{
    /// <summary>
    /// ViewModel dealing with files and bookmarks up for extraction. Sends info and receives it from
    /// <see cref="ExtractionOrderViewModel"/>.
    /// </summary>
    public class ExtractionViewModel : ViewModelBaseLogging<ExtractionViewModel>, INavigationTarget
    {
        #region DI services
        private readonly IExtractionExecutor executor;
        private readonly IPathSelection Input;
        private readonly IDialogAssist dialogAssist;
        private readonly IEventAggregator eventAggregator;
        private readonly IBookmarkService bookmarkService;
        private readonly IConfiguration configuration;
        #endregion

        #region Properties and fields
        // Store last known file path as the starting point for user's directory destination selection
        private string currentFilePath;

        /// <summary>
        /// Collection for storing info on files, whose bookmarks can be selected and extracted.
        /// </summary>
        public ObservableCollection<FileAndBookmarksStorage> Files { get; private set; }
        private FileAndBookmarksStorage selectedFile;

        /// <summary>
        /// The file that is being currently worked on. Notify properties on change.
        /// </summary>
        public FileAndBookmarksStorage SelectedFile
        {
            get => selectedFile;
            set
            {
                SetProperty(ref selectedFile, value);
                RaisePropertyChanged(nameof(FileBookmarks));
            }
        }

        /// <summary>
        /// The collection of bookmarks from the currently selected file (or null, if no file selected or no bookmarks).
        /// </summary>
        public ObservableCollection<FileAndBookmarkWrapper> FileBookmarks
        {
            get =>
                SelectedFile != null && SelectedFile.Bookmarks != null
                    ? SelectedFile.Bookmarks
                    : null;
        }

        private FileAndBookmarkWrapper selectedBookmark;

        /// <summary>
        /// The bookmark that has been last selected by user.
        /// </summary>
        public FileAndBookmarkWrapper SelectedBookmark
        {
            get { return selectedBookmark; }
            set { SetProperty(ref selectedBookmark, value); }
        }
        #endregion

        #region Subscription tokens
        // Hold tokens for events that have been subscribed to (in order to unsubscribe when leaving).
        SubscriptionToken filesAddedSubscription;
        SubscriptionToken bookmarkDeselectedSubscription;
        #endregion

        #region Constructor
        /// <summary>
        /// ViewModel for handling selection of extractable bookmarks in files.
        /// </summary>
        /// <param name="eventAggregator">Service for publishing and receiving events between viewModels.</param>
        /// <param name="input">Service for getting a file or folderpath from a user.</param>
        /// <param name="dialogAssist">Service for showing and otherwise handling dialogs.</param>
        /// <param name="navregistry">Navigation registry for viewModels.</param>
        /// <param name="executor">Service for performing bookmark extraction.</param>
        /// <param name="bookmarkService">Service for retrieving and manipulating bookmarks.</param>
        /// <param name="configuration">Program-wide configurations.</param>
        /// <param name="logbook">Loggin service.</param>
        public ExtractionViewModel(
            IEventAggregator eventAggregator,
            IPathSelection input,
            IDialogAssist dialogAssist,
            INavigationTargetRegistry navregistry,
            IExtractionExecutor executor,
            IBookmarkService bookmarkService,
            IConfiguration configuration,
            ILogbook logbook
        ) : base(logbook)
        {
            // Assign services and associate this viewmodel with
            // a navigation scheme.

            this.eventAggregator = eventAggregator;
            ;
            Input = input;
            this.dialogAssist = dialogAssist;
            this.executor = executor;
            this.bookmarkService = bookmarkService;
            this.configuration = configuration;
            navregistry.AddTarget(SchemeNames.EXTRACT, this);

            // Initialize the collection of files.

            Files = new ObservableCollection<FileAndBookmarksStorage>();
        }
        #endregion

        #region INavigationTarget implementation
        /// <summary>
        /// See <see cref="INavigationTarget"/>, <see cref="INavigationTargetRegistry"/> and
        /// <see cref="INavigationAssist"/> for more information.
        /// </summary>
        public void OnArrival()
        {
            // This ViewModel needs to know when the user has selected new files and if some bookmarks have been
            // unselected (in the Extraction Order ViewModel - in order to redisplay them here).

            // Subscribe to events that inform of the abovementioned cases

            filesAddedSubscription = eventAggregator
                .GetEvent<FilesAddedEvent>()
                .Subscribe(FilesAddedHandler);
            bookmarkDeselectedSubscription = eventAggregator
                .GetEvent<BookmarkDeselectedEvent>()
                .Subscribe(BookmarkDeselectedHandler);

            logbook.Write($"{this} subscribed to {nameof(FilesAddedEvent)}.", LogLevel.Debug);
            logbook.Write(
                $"{this} subscribed to {nameof(BookmarkDeselectedEvent)}.",
                LogLevel.Debug
            );
        }

        /// <summary>
        /// See <see cref="INavigationTarget"/>, <see cref="INavigationTargetRegistry"/> and
        /// <see cref="INavigationAssist"/> for more information.
        /// </summary>
        public void WhenLeaving()
        {
            // Unsubscribe from file and unselection notifications.

            eventAggregator.GetEvent<FilesAddedEvent>().Unsubscribe(filesAddedSubscription);
            eventAggregator
                .GetEvent<BookmarkDeselectedEvent>()
                .Unsubscribe(bookmarkDeselectedSubscription);

            logbook.Write($"{this} unsubscribed from {nameof(FilesAddedEvent)}.", LogLevel.Debug);
            logbook.Write(
                $"{this} unsubscribed from {nameof(BookmarkDeselectedEvent)}.",
                LogLevel.Debug
            );
        }

        /// <summary>
        /// See <see cref="INavigationTarget"/>, <see cref="INavigationTargetRegistry"/> and
        /// <see cref="INavigationAssist"/> for more information.
        /// </summary>
        public void Reset()
        {
            // When resetting, clear all added files and and bookmarks from the lists. And clear selected file.

            Files.Clear();
            FileBookmarks?.Clear();
            SelectedFile = null;
        }

        /// <summary>
        /// Handler for dealing with file additions (through file addition event)
        /// </summary>
        /// <param name="filePaths">Filepaths of the added files</param>
        private async void FilesAddedHandler(string[] filePaths)
        {
            foreach (string path in filePaths)
            {
                // Only add files if they are new

                if (Files.Any(f => f.FilePath == path) == false)
                {
                    FileAndBookmarksStorage storage = new FileAndBookmarksStorage(path);

                    // Find bookmarks for each file and store them in a wrapper along with file information

                    foreach (ILeveledBookmark found in await bookmarkService.FindBookmarks(path))
                    {
                        storage.Bookmarks.Add(new FileAndBookmarkWrapper(found, path));
                    }
                    Files.Add(storage);
                }
            }

            // If only one file was added, select it. Otherwise keep selection.

            if (filePaths.Count() == 1)
            {
                SelectedFile = Files.Last();
            }
            else if (SelectedFile == null)
            {
                SelectedFile = Files.FirstOrDefault();
            }

            // Store the last filepath for future reference
            currentFilePath = filePaths.LastOrDefault();
        }

        /// <summary>
        /// Handler for dealing with deselection of bookmarks (received from <see cref="ExtractionOrderViewModel"/>).
        /// </summary>
        /// <param name="id">Id of the deselected bookmark.</param>
        private void BookmarkDeselectedHandler(Guid id)
        {
            // Search for the correct bookmark in all files

            foreach (FileAndBookmarksStorage file in Files)
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
                    }
                }
            }
        }
        #endregion

        #region Commands
        private IAsyncCommand addCommand;

        /// <summary>
        /// Command for adding new bookmarks.
        /// </summary>
        public IAsyncCommand AddCommand =>
            addCommand ?? (addCommand = new AsyncCommand(ExecuteAddCommand));

        /// <summary>
        /// Execution method for bookmark addition command, see <see cref="AddCommand"/>
        /// </summary>
        /// <returns></returns>
        protected async Task ExecuteAddCommand()
        {
            // Create a dialog for user-provided options and show said dialog.

            BookmarkDialog dialog = new BookmarkDialog(Resources.Labels.Dialogs.Bookmark.New)
            {
                StartPage = 1,
                EndPage = 1
            };

            await dialogAssist.Show(dialog);

            // The user canceled, just return.

            if (dialog.IsCanceled)
            {
                logbook.Write(
                    $"Cancellation requested at {nameof(IDialog)} '{dialog.DialogTitle}'.",
                    LogLevel.Information
                );

                return;
            }

            // The user confirmed, create new info for adding a bookmark and call appropriate method.

            BookmarkInfo info = new BookmarkInfo(
                dialog.StartPage,
                dialog.EndPage,
                dialog.Title,
                SelectedFile.FilePath
            );
            BookmarkAdded(info);

            logbook.Write($"{nameof(BookmarkInfo)} '{info.Title}' added.", LogLevel.Information);
        }

        private DelegateCommand<SelectionChangedEventArgs> selectionCommand;

        /// <summary>
        /// Command for bookmark selection changes
        /// </summary>
        public DelegateCommand<SelectionChangedEventArgs> SelectionCommand =>
            selectionCommand
            ?? (
                selectionCommand = new DelegateCommand<SelectionChangedEventArgs>(
                    ExecuteSelectionCommand
                )
            );

        /// <summary>
        /// Execution method for selection change command, see <see cref="SelectionCommand"/>
        /// </summary>
        /// <param name="parameter">Event arguments of changed selection.</param>
        protected void ExecuteSelectionCommand(SelectionChangedEventArgs parameter)
        {
            if (parameter.AddedItems.Count > 0)
            {
                // Select the children of the selected bookmark (because they are included in the
                // page range of the parent).

                // All selected bookmarks will be hidden.

                FileAndBookmarkWrapper wrapper = parameter.AddedItems[0] as FileAndBookmarkWrapper;

                SelectChildrenRecursively(wrapper);
                BookmarkInfo info = new BookmarkInfo(
                    wrapper.Bookmark.StartPage,
                    wrapper.Bookmark.EndPage,
                    wrapper.Bookmark.Title,
                    wrapper.FilePath,
                    wrapper.Id
                );

                // Send info of the selected bookmark to Extraction Order ViewModel through events.

                eventAggregator.GetEvent<BookmarkSelectedEvent>().Publish(info);
            }
        }

        private DelegateCommand viewFileCommand;

        /// <summary>
        /// Command for viewing the selected file in an external program.
        /// </summary>
        public DelegateCommand ViewFileCommand =>
            viewFileCommand ?? (viewFileCommand = new DelegateCommand(ExecuteViewFileCommand));

        /// <summary>
        /// Execution method for file viewing command, see <see cref="ViewFileCommand"/>
        /// </summary>
        protected void ExecuteViewFileCommand()
        {
            if (SelectedFile != null)
            {
                // Open the file in system default program

                new System.Diagnostics.Process()
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo(SelectedFile.FilePath)
                    {
                        UseShellExecute = true
                    }
                }.Start();
            }

            logbook.Write($"{SelectedFile.FileName} opened externally.", LogLevel.Information);
        }

        private DelegateCommand deleteFileCommand;

        /// <summary>
        /// Command from deleting the file from the list
        /// </summary>
        public DelegateCommand DeleteFileCommand =>
            deleteFileCommand
            ?? (deleteFileCommand = new DelegateCommand(ExecuteDeleteFileCommand));

        /// <summary>
        /// Execution method for file deletion command, see <see cref="DeleteFileCommand"/>
        /// </summary>
        protected void ExecuteDeleteFileCommand()
        {
            if (SelectedFile != null)
            {
                // Remove the selected file from the collection

                int index = Files.IndexOf(SelectedFile);
                string path = SelectedFile.FilePath;
                Files.Remove(SelectedFile);

                // Publish an event to inform a file has been removed (Extract Order ViewModel needs said info
                // to remove selected bookmarks associated with given file).

                eventAggregator.GetEvent<BookmarkFileDeletedEvent>().Publish(path);

                // Select another file based on how many files there are left

                if (Files.Count > 0)
                {
                    if (index == 0 || Files.Count == 1)
                    {
                        SelectedFile = Files[0];
                    }
                    else
                    {
                        SelectedFile = Files[index - 1];
                    }
                }
                else
                {
                    SelectedFile = null;
                }
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Method for handling new user requested bookmarks.
        /// <para>
        /// Bookmark will be added to the hierarchy of current documents bookmarks and immediately selected.
        /// </para>
        /// </summary>
        /// <param name="input">Info for the new bookmark</param>
        protected void BookmarkAdded(BookmarkInfo input)
        {
            // Sort the bookmarks in order in a new list. Find the parent of the newly added bookmark,
            // if it has a parent.

            List<FileAndBookmarkWrapper> sorted = FileBookmarks
                .OrderBy(x => x.Bookmark.StartPage)
                .ToList();
            FileAndBookmarkWrapper parent = FileAndBookmarkWrapper.FindParent(
                sorted,
                input.StartPage,
                input.EndPage
            );
            int level = parent == null ? 1 : parent.Bookmark.Level + 1;
            FileAndBookmarkWrapper addMark = new FileAndBookmarkWrapper(
                new LeveledBookmark(
                    level,
                    input.Title,
                    input.StartPage,
                    input.EndPage - input.StartPage + 1
                ),
                input.FilePath
            );

            // Find out if the new bookmark is the first sibling or if there are siblings higher up in the bookmarks tree.
            // Also find out, if the new bookmark will have children (in order to adjust their levels).

            FileAndBookmarkWrapper precedingSibling = addMark.FindPrecedingSibling(sorted, parent);
            IList<FileAndBookmarkWrapper> children = addMark.FindChildren(sorted);

            // Default to first bookmark

            int index = 0;

            // Sub-level bookmark, but first of its kind

            if (parent != null && precedingSibling == null)
                index = FileBookmarks.IndexOf(parent) + 1;

            // Not the first of its kind, top-level or sub-level

            if (precedingSibling != null)
                index = FileBookmarks.IndexOf(precedingSibling) + 1;

            // Add bookmark

            addMark.IsSelected = true;
            FileBookmarks.Insert(index, addMark);

            // Adjust childrens' info, if there are any.

            foreach (FileAndBookmarkWrapper child in children)
            {
                int childIndex = FileBookmarks.IndexOf(child);
                FileBookmarks.RemoveAt(childIndex);
                FileBookmarks.Insert(
                    childIndex,
                    new FileAndBookmarkWrapper(
                        new LeveledBookmark(
                            child.Bookmark.Level + 1,
                            child.Bookmark.Title,
                            child.Bookmark.Pages
                        ),
                        SelectedFile.FilePath
                    )
                );
            }
        }

        /// <summary>
        /// Select all children of a bookmark in a recursive manner.
        /// </summary>
        /// <param name="mark">Bookmark whose children should be selected</param>
        protected void SelectChildrenRecursively(FileAndBookmarkWrapper mark)
        {
            foreach (FileAndBookmarkWrapper child in mark.FindChildren(FileBookmarks))
            {
                child.IsSelected = true;
                SelectChildrenRecursively(child);
            }
        }

        /// <summary>
        /// Deselect all children of a bookmark in a recursive manner.
        /// </summary>
        /// <param name="mark">Bookmark whose children should be deselected</param>
        /// <param name="bookmarks">Bookmarks to look the children in</param>
        protected void DeSelectChildrenRecursively(
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
        protected void DeSelectParent(
            FileAndBookmarkWrapper mark,
            ObservableCollection<FileAndBookmarkWrapper> bookmarks
        )
        {
            FileAndBookmarkWrapper parent = mark.FindParent(bookmarks);
            if (parent != null)
            {
                parent.IsSelected = false;
            }
        }
        #endregion
    }
}
