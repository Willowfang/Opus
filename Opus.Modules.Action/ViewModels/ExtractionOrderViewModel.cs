using AsyncAwaitBestPractices.MVVM;
using CX.LoggingLib;
using Opus.Events.Data;
using Opus.Services.Implementation.Data.Extraction;
using Opus.Services.Implementation.StaticHelpers;
using Opus.Services.Implementation.UI.Dialogs;
using Opus.Services.UI;
using Prism.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Events;
using Opus.Core.Executors;
using Opus.Core.Base;
using CX.PdfLib.Common;
using Opus.Events;
using Opus.Values;
using Opus.Services.Extensions;
using System.IO;
using Opus.Services.Configuration;
using Opus.Services.Input;
using CX.PdfLib.Extensions;
using Opus.Services.Implementation.Extensions;

namespace Opus.Modules.Action.ViewModels
{
    /// <summary>
    /// ViewModel for organizing bookmarks for extraction. Depends on more general extraction services.
    /// Communicates (via events) with <see cref="ExtractionViewModel"/>.
    /// </summary>
    public class ExtractionOrderViewModel
        : ViewModelBaseLogging<ExtractionOrderViewModel>,
            INavigationTarget
    {
        #region DI services
        private readonly IEventAggregator eventAggregator;
        private readonly IDialogAssist dialogAssist;
        private readonly IExtractionExecutor executor;
        private readonly IConfiguration configuration;
        private readonly IPathSelection pathSelection;
        #endregion

        #region Properties and fields
        /// <summary>
        /// Collection for the bookmarks to extract.
        /// </summary>
        /// <remarks>See <see cref="ReorderCollection{T}"/> for more information on properties and methods on organizing the bookmarks before extraction.</remarks>
        public ReorderCollection<FileAndBookmarkWrapper> Bookmarks { get; }

        /// <summary>
        /// Check whether currently selected item is an actual bookmark from a document (rather than a placeholder or null).
        /// Changes are notified when selected item changes.
        /// </summary>
        public bool IsSelectedActualBookmark
        {
            get =>
                Bookmarks.SelectedItem != null && Bookmarks.SelectedItem.Bookmark.Pages.Count > 0;
        }

        /// <summary>
        /// Check whether the collection contains´any items and whether any of those items are actual bookmarks
        /// from a document (rather than just placeholders). Changes are notified on collection changes.
        /// </summary>
        public bool CollectionHasActualBookmarks
        {
            get
            {
                bool value = false;
                foreach (FileAndBookmarkWrapper wrapper in Bookmarks)
                {
                    if (wrapper.Bookmark.Pages.Count > 0)
                    {
                        value = true;
                        break;
                    }
                }
                return value;
            }
        }
        #endregion

        #region Subscription tokens
        // Tokens for holding event subscriptions. Subscriptions will be canceled when the user navigates away.
        SubscriptionToken bookmarkEventSubscription;
        SubscriptionToken bookmarkFileDeletedEventSubscription;
        #endregion

        #region Constructor
        /// <summary>
        /// ViewModel for extraction bookmark ordering.
        /// </summary>
        /// <remarks>Parameters are received through dependency injection</remarks>
        /// <param name="eventAggregator">Service for publishing and receiving events between viewModels.</param>
        /// <param name="dialogAssist">Service for showing and otherwise handling dialogs.</param>
        /// <param name="navregistry">Navigation registry for viewModels.</param>
        /// <param name="executor">Service for performing the actual extraction.</param>
        /// <param name="configuration">Program-wide configurations.</param>
        /// <param name="pathSelection">Service for user file or folder pathselection.</param>
        /// <param name="logbook">Logging service.</param>
        public ExtractionOrderViewModel(
            IEventAggregator eventAggregator,
            IDialogAssist dialogAssist,
            INavigationTargetRegistry navregistry,
            IExtractionExecutor executor,
            IConfiguration configuration,
            IPathSelection pathSelection,
            ILogbook logbook
        ) : base(logbook)
        {
            // Assign services and subscribe to a navigation scheme. This view will be navigated to
            // when extraction view is requested by the user.
            this.eventAggregator = eventAggregator;
            this.dialogAssist = dialogAssist;
            this.executor = executor;
            this.configuration = configuration;
            this.pathSelection = pathSelection;
            navregistry.AddTarget(SchemeNames.EXTRACT, this);

            // Initialize bookmark collection. Reordering of the collection is possible either by
            // dragging and dropping or by keyboard.
            Bookmarks = new ReorderCollection<FileAndBookmarkWrapper>();
            Bookmarks.CanReorder = true;

            // Subscribe to various change events of the collection.
            Bookmarks.CollectionReordered += (sender, args) => UpdateEntries();
            Bookmarks.CollectionItemAdded += (sender, args) => UpdateEntries();
            Bookmarks.CollectionChanged += Bookmarks_CollectionChanged;
            Bookmarks.CollectionSelectedItemChanged += BookmarksSelectedItemChangedHandler;
        }
        #endregion

        #region INavigationTarget implementation
        /// <summary>
        /// See <see cref="INavigationTarget"/>, <see cref="INavigationTargetRegistry"/> and
        /// <see cref="INavigationAssist"/> for more information.
        /// </summary>
        public void OnArrival()
        {
            // Extraction ViewModel sends selected bookmarks and files that have been deleted from the list
            // as following events. Subscribe to said events and execute appropriate methods when needed.

            bookmarkEventSubscription = eventAggregator
                .GetEvent<BookmarkSelectedEvent>()
                .Subscribe(BookmarkSelectedHandler);
            bookmarkFileDeletedEventSubscription = eventAggregator
                .GetEvent<BookmarkFileDeletedEvent>()
                .Subscribe(BookmarkFileDeletedHandler);

            logbook.Write($"{this} subscribed to {nameof(BookmarkSelectedEvent)}.", LogLevel.Debug);
        }

        /// <summary>
        /// See <see cref="INavigationTarget"/>, <see cref="INavigationTargetRegistry"/> and
        /// <see cref="INavigationAssist"/> for more information.
        /// </summary>
        public void WhenLeaving()
        {
            // Unsubscribe from events sent by Extraction ViewModel.

            eventAggregator
                .GetEvent<BookmarkSelectedEvent>()
                .Unsubscribe(bookmarkEventSubscription);
            eventAggregator
                .GetEvent<BookmarkFileDeletedEvent>()
                .Unsubscribe(bookmarkFileDeletedEventSubscription);

            logbook.Write(
                $"{this} unsubscribed from {nameof(BookmarkSelectedEvent)}.",
                LogLevel.Debug
            );
        }

        /// <summary>
        /// See <see cref="INavigationTarget"/>, <see cref="INavigationTargetRegistry"/> and
        /// <see cref="INavigationAssist"/> for more information.
        /// </summary>
        public void Reset()
        {
            // When a reset has been requested by the user, clear all bookmarks from the collection.

            Bookmarks.Clear();
        }

        /// <summary>
        /// Handler for bookmark selection events published by <see cref="ExtractionViewModel"/>.
        /// </summary>
        /// <param name="info">Information basis for a new selected bookmark</param>
        private void BookmarkSelectedHandler(BookmarkInfo info)
        {
            // Check whether the bookmark already exist in the collection. If it does,
            // do not add it again.
            if (Bookmarks.FirstOrDefault(b => b.Id == info.Id) != null)
                return;

            // Create the wrapper.
            FileAndBookmarkWrapper wrapper = new FileAndBookmarkWrapper(
                new LeveledBookmark(
                    1,
                    info.Title,
                    info.StartPage,
                    info.EndPage - info.StartPage + 1
                ),
                info.FilePath,
                0,
                info.Id
            );

            // Check whether the bookmark has a parent in the collection. If it does, do
            // not add the bookmark (it is already included in the page range of the parent).
            List<FileAndBookmarkWrapper> relatedBookmarks = Bookmarks
                .Where(b => b.FilePath == wrapper.FilePath)
                .ToList();
            FileAndBookmarkWrapper parent = wrapper.FindParent(relatedBookmarks);

            if (parent == null)
            {
                // Add new bookmark
                Bookmarks.Push(wrapper);

                // If a new parent is added, remove all children of said parent from the list (they are
                // included in the range of the parent)
                IList<FileAndBookmarkWrapper> children = wrapper.FindChildren(relatedBookmarks);
                foreach (FileAndBookmarkWrapper child in children)
                {
                    Bookmarks.Remove(child);
                }
            }
        }

        /// <summary>
        /// Handler for file deletion events published by <see cref="ExtractionViewModel"/>.
        /// </summary>
        /// <param name="path">Filepath of the file that was removed by the user</param>
        private void BookmarkFileDeletedHandler(string path)
        {
            // Remove all bookmarks with a corresponding path from the collection (since
            // the relevant document was also removed).
            Bookmarks.RemoveAll(b => b.FilePath == path);
        }
        #endregion

        #region Collection change handlers
        /// <summary>
        /// Handle changes in the selected item of the collection. Notifies relevant
        /// properties of the change, updating UI.
        /// </summary>
        /// <param name="sender">Sending collection</param>
        /// <param name="e"><see cref="CollectionSelectedItemChangedEventArgs{T}"/></param>
        private void BookmarksSelectedItemChangedHandler(
            object sender,
            CollectionSelectedItemChangedEventArgs<FileAndBookmarkWrapper> e
        )
        {
            RaisePropertyChanged(nameof(IsSelectedActualBookmark));
        }

        /// <summary>
        /// Handle all changes in the collection. Updates entries (their <see cref="FileAndBookmarkWrapper.Index"/>
        /// property) and produces relevant notifications.
        /// </summary>
        /// <param name="sender">Sending collection</param>
        /// <param name="e">Event arguments</param>
        private void Bookmarks_CollectionChanged(
            object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e
        )
        {
            UpdateEntries();
        }
        #endregion

        #region Commands
        private DelegateCommand addExternalCommand;

        /// <summary>
        /// Command for adding a placeholder for external files (for numbering purposes)
        /// </summary>
        public DelegateCommand AddExternalCommand =>
            addExternalCommand ??= new DelegateCommand(ExecuteAddExternalCommand);

        /// <summary>
        /// External addition execution method, see <see cref="AddExternalCommand"/>.
        /// </summary>
        protected void ExecuteAddExternalCommand()
        {
            // Insert an empty placeholder bookmark at the end of the collection
            Bookmarks.Add(
                BookmarkMethods.GetPlaceHolderBookmarkWrapper(
                    Resources.Labels.Dialogs.ExtractionOrder.ExternalFile,
                    Resources.Labels.Dialogs.ExtractionOrder.ExternalFile,
                    Bookmarks.Count + 1
                )
            );
        }

        private IAsyncCommand editCommand;

        /// <summary>
        /// Command for editing a selected bookmark (other than a placeholder)
        /// </summary>
        public IAsyncCommand EditCommand => editCommand ??= new AsyncCommand(ExecuteEditCommand);

        /// <summary>
        /// Execution method for edition command, see <see cref="EditCommand"/>
        /// </summary>
        /// <returns></returns>
        protected async Task ExecuteEditCommand()
        {
            if (Bookmarks.SelectedItem == null)
            {
                return;
            }

            // Show a dialog with editing options to the user. Prefill with relevant info.

            BookmarkDialog dialog = new BookmarkDialog(Resources.Labels.Dialogs.Bookmark.Edit)
            {
                Title = Bookmarks.SelectedItem.Bookmark.Title,
                StartPage = Bookmarks.SelectedItem.Bookmark.StartPage,
                EndPage = Bookmarks.SelectedItem.Bookmark.EndPage
            };

            await dialogAssist.Show(dialog);

            // The user canceled, just return

            if (dialog.IsCanceled)
            {
                logbook.Write(
                    $"Cancellation requested at {nameof(IDialog)} '{dialog}'.",
                    LogLevel.Information
                );
                return;
            }

            // The user accepted, create a new wrapper for the bookmark. Remove currently selected bookmark
            // and insert edited version at the index of the original.

            FileAndBookmarkWrapper updated = new FileAndBookmarkWrapper(
                new LeveledBookmark(
                    Bookmarks.SelectedItem.Bookmark.Level,
                    dialog.Title,
                    dialog.StartPage,
                    dialog.EndPage - dialog.StartPage + 1
                ),
                Bookmarks.SelectedItem.FilePath,
                Bookmarks.SelectedItem.Index,
                Bookmarks.SelectedItem.Id
            );

            int index = Bookmarks.IndexOf(Bookmarks.SelectedItem);
            Bookmarks.RemoveSelected();
            Bookmarks.Insert(index, updated);

            logbook.Write(
                $"{nameof(BookmarkInfo)} '{updated.Bookmark.Title}' edited.",
                LogLevel.Information
            );
        }

        private DelegateCommand deleteCommand;

        /// <summary>
        /// Command for deleting the selected bookmark.
        /// </summary>
        public DelegateCommand DeleteCommand =>
            deleteCommand ?? (deleteCommand = new DelegateCommand(ExecuteDeleteCommand));

        /// <summary>
        /// Execution method for deletion command, see <see cref="DeleteCommand"/>
        /// </summary>
        protected void ExecuteDeleteCommand()
        {
            // Publish a bookmark deletion event with relevant info (Guid of the bookmark). Will
            // be picked up at Extraction ViewModel, where a bookmark with the same Guid will be made visible.

            eventAggregator
                .GetEvent<BookmarkDeselectedEvent>()
                .Publish(Bookmarks.SelectedItem.Id);

            // Remove selected bookmark from the collection and if - after this - the collection contains no
            // other instances besides placeholders, clear it (no need to preserve just placeholders).

            Bookmarks.RemoveSelected();
            if (Bookmarks.All(b => b.Bookmark.Pages.Count == 0))
            {
                Bookmarks.Clear();
            }
        }

        private IAsyncCommand saveFileCommand;

        /// <summary>
        /// Command for saving into a single file.
        /// </summary>
        public IAsyncCommand SaveFileCommand =>
            saveFileCommand ?? (saveFileCommand = new AsyncCommand(ExecuteSaveFileCommand));

        /// <summary>
        /// Execution method for saving into single file command, see <see cref="saveFileCommand"/>
        /// </summary>
        /// <returns></returns>
        protected async Task ExecuteSaveFileCommand()
        {
            // Saving into a zip-file requested

            if (configuration.ExtractionCreateZip)
            {
                await SaveAsZip(true);
                return;
            }

            // Ask the user for save path for the file.

            string path = pathSelection.SaveFile(
                Resources.UserInput.Descriptions.SelectSaveFile,
                FileType.PDF,
                new DirectoryInfo(Path.GetDirectoryName(Bookmarks[0].FilePath))
            );
            if (path == null)
                return;

            FileInfo destination = new FileInfo(path);

            logbook.Write($"Starting extraction to file.", LogLevel.Information);

            // Execute extraction

            await executor.Save(destination, Bookmarks.Extractables());

            logbook.Write($"Extraction finished.", LogLevel.Information);
        }

        private IAsyncCommand saveSeparateCommand;

        /// <summary>
        /// Command for saving bookmarks in separate files.
        /// </summary>
        public IAsyncCommand SaveSeparateCommand =>
            saveSeparateCommand
            ?? (saveSeparateCommand = new AsyncCommand(ExecuteSaveSeparateCommand));

        /// <summary>
        /// Execution method for separate saving command, see <see cref="SaveSeparateCommand"/>
        /// </summary>
        /// <returns></returns>
        protected async Task ExecuteSaveSeparateCommand()
        {
            // Saving into a zip-file requested.

            if (configuration.ExtractionCreateZip)
            {
                await SaveAsZip(false);
                return;
            }

            // Ask the user for a save path

            string path = pathSelection.OpenDirectory(
                Resources.UserInput.Descriptions.SelectSaveFolder
            );
            if (path == null)
                return;

            DirectoryInfo destination = new DirectoryInfo(path);

            logbook.Write($"Starting extraction to directory.", LogLevel.Information);

            // Execute extraction

            await executor.Save(destination, Bookmarks.Extractables());

            logbook.Write($"Extraction finished.", LogLevel.Information);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Update info on all collection items (e.g. <see cref="FileAndBookmarkWrapper.Index"/> property)
        /// and raise relevant property notifications.
        /// </summary>
        public void UpdateEntries()
        {
            for (int i = 0; i < Bookmarks.Count; i++)
            {
                Bookmarks[i].Index = i + 1;
            }

            RaisePropertyChanged(nameof(CollectionHasActualBookmarks));
        }

        /// <summary>
        /// Save extracted bookmarks into files and compress them in a zip-file.
        /// </summary>
        /// <param name="saveSingular">If true, extract bookmarks into one file (instead of each one in a separate file)</param>
        /// <returns></returns>
        private async Task SaveAsZip(bool saveSingular)
        {
            // The destination path

            FileSystemInfo destination;

            // Create a temporary, randomly named directory in the TEMP directory to hold the extracted files
            // before they are compressed.

            DirectoryInfo tempDir = new DirectoryInfo(
                Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())
            );
            tempDir.Create();

            // Save bookmarks into a single file.

            if (saveSingular)
            {
                // Ask the user for the name of the zip-file and name of the contained file.

                ExtractSettingsDialog fileNameDialog = new ExtractSettingsDialog(
                    Resources.Labels.Dialogs.ExtractionOptions.ZipDialogTitle,
                    true,
                    Resources.Labels.Dialogs.ExtractionOptions.ZipName,
                    Resources.Labels.Dialogs.ExtractionOptions.ZipNameHelper,
                    false
                );

                await dialogAssist.Show(fileNameDialog);

                // User has cancelled, just return.

                if (fileNameDialog.IsCanceled)
                    return;

                // Get user entered filename for the zip and replace unallowed characters in the filename.

                string fileName =
                    Path.GetFileNameWithoutExtension(fileNameDialog.Title.ReplaceIllegal())
                    + ".pdf";
                string fullPath = Path.Combine(tempDir.FullName, fileName);

                // Assign full path to destination path.

                destination = new FileInfo(fullPath);
            }
            // Save bookmarks in separate files (store them in the temp directory before compressing).

            else
            {
                destination = tempDir;
            }

            // Ask for the zip-file destination from the user.

            string zipPath = pathSelection.SaveFile(
                Resources.UserInput.Descriptions.SelectSaveFile,
                FileType.Zip,
                new DirectoryInfo(Path.GetDirectoryName(Bookmarks[0].FilePath))
            );
            FileInfo zipFile = new FileInfo(zipPath);

            logbook.Write($"Starting extraction to zip-file.", LogLevel.Information);

            // Extract and create zip.

            await executor.SaveAsZip(destination, Bookmarks.Extractables(), zipFile);
            await Task.Run(() => tempDir.Delete(true));

            logbook.Write($"Extraction finished.", LogLevel.Information);
        }

        #endregion
    }
}
