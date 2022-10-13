using System.Collections.ObjectModel;
using Prism.Events;
using Prism.Commands;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using Opus.Core.Base;
using CX.PdfLib.Services;
using CX.PdfLib.Services.Data;
using CX.PdfLib.Common;
using Opus.Core.Wrappers;
using Opus.Events.Data;
using System.Threading.Tasks;
using AsyncAwaitBestPractices.MVVM;
using Opus.Values;
using Opus.Services.Input;
using Opus.Events;
using Opus.Services.UI;
using Opus.Services.Implementation.UI.Dialogs;
using Opus.Services.Extensions;
using Opus.Core.Executors;
using CX.LoggingLib;
using CX.PdfLib.Extensions;
using Opus.Services.Configuration;
using Opus.Services.Implementation.Data.Extraction;
using System;

namespace Opus.Modules.Action.ViewModels
{
    public class ExtractionViewModel : ViewModelBaseLogging<ExtractionViewModel>, INavigationTarget
    {
        private readonly IExtractionExecutor executor;
        private readonly IPathSelection Input;
        private readonly IDialogAssist dialogAssist;
        private readonly IEventAggregator eventAggregator;
        private readonly IBookmarkService bookmarkService;
        private readonly IConfiguration configuration;
        private string currentFilePath;

        public ObservableCollection<FileAndBookmarksStorage> Files { get; private set; }
        private FileAndBookmarksStorage selectedFile;
        public FileAndBookmarksStorage SelectedFile
        {
            get => selectedFile;
            set
            {
                SetProperty(ref selectedFile, value);
                if (value != null)
                    FileBookmarks = selectedFile.Bookmarks;
                else
                    FileBookmarks = null;

                FadeInList = true;
                FadeInList = false;
            }
        }

        private ObservableCollection<FileAndBookmarkWrapper> fileBookmarks;
        public ObservableCollection<FileAndBookmarkWrapper> FileBookmarks
        {
            get => fileBookmarks;
            set => SetProperty(ref fileBookmarks, value);
        }

        private FileAndBookmarkWrapper selectedBookmark;
        public FileAndBookmarkWrapper SelectedBookmark
        {
            get { return selectedBookmark; }
            set { SetProperty(ref selectedBookmark, value); }
        }

        private bool isFileSelected;
        public bool IsFileSelected
        {
            get => isFileSelected;
            set
            {
                SetProperty(ref isFileSelected, value);
            }
        }

        private bool fadeInList;
        public bool FadeInList
        {
            get => fadeInList;
            set => SetProperty(ref fadeInList, value);
        }

        public ExtractionViewModel(
            IEventAggregator eventAggregator,
            IPathSelection input, 
            IDialogAssist dialogAssist,
            INavigationTargetRegistry navregistry, 
            IExtractionExecutor executor,
            IBookmarkService bookmarkService,
            IConfiguration configuration,
            ILogbook logbook) : base(logbook)
        {
            this.eventAggregator = eventAggregator;
            Files = new ObservableCollection<FileAndBookmarksStorage>();
            FileBookmarks = new ObservableCollection<FileAndBookmarkWrapper>();
            Input = input;
            this.dialogAssist = dialogAssist;
            this.executor = executor;
            this.bookmarkService = bookmarkService;
            this.configuration = configuration;
            navregistry.AddTarget(SchemeNames.SPLIT, this);
        }

        SubscriptionToken filesAddedSubscription;
        SubscriptionToken bookmarkDeselectedSubscription;
        public void OnArrival()
        {
            filesAddedSubscription = eventAggregator.GetEvent<FilesAddedEvent>().Subscribe(FilesAdded);
            bookmarkDeselectedSubscription = eventAggregator.GetEvent<BookmarkDeselectedEvent>().Subscribe(BookmarkDeselected);

            logbook.Write($"{this} subscribed to {nameof(FilesAddedEvent)}.", LogLevel.Debug);
            logbook.Write($"{this} subscribed to {nameof(BookmarkDeselectedEvent)}.", LogLevel.Debug);
        }
        public void WhenLeaving()
        {
            eventAggregator.GetEvent<FilesAddedEvent>().Unsubscribe(filesAddedSubscription);
            eventAggregator.GetEvent<BookmarkDeselectedEvent>().Unsubscribe(bookmarkDeselectedSubscription);

            logbook.Write($"{this} unsubscribed from {nameof(FilesAddedEvent)}.", LogLevel.Debug);
            logbook.Write($"{this} unsubscribed from {nameof(BookmarkDeselectedEvent)}.", LogLevel.Debug);
        }
        public void Reset()
        {
            Files.Clear();
            FileBookmarks?.Clear();
            SelectedFile = null;
        }

        private async void FilesAdded(string[] filePaths)
        {
            foreach (string path in filePaths)
            {
                if (Files.Any(f => f.FilePath == path) == false)
                {
                    FileAndBookmarksStorage storage = new FileAndBookmarksStorage(path);
                    foreach (ILeveledBookmark found in await bookmarkService.FindBookmarks(path))
                    {
                        storage.Bookmarks.Add(new FileAndBookmarkWrapper(found, path));
                    }
                    Files.Add(storage);
                }
            }

            if (SelectedFile == null)
                SelectedFile = Files.FirstOrDefault();

            IsFileSelected = true;
            currentFilePath = filePaths.LastOrDefault();
        }
        private void BookmarkDeselected(Guid id)
        {
            foreach (FileAndBookmarksStorage file in Files)
            {
                foreach (FileAndBookmarkWrapper bookmark in file.Bookmarks)
                {
                    if (bookmark.Id == id)
                    {
                        DeSelectParent(bookmark, file.Bookmarks);
                        bookmark.IsSelected = false;
                        DeSelectChildrenRecursively(bookmark, file.Bookmarks);
                    }
                }
            }
        }

        private void BookmarkAdded(BookmarkInfo input)
        {
            // Sort the bookmarks in order in a new list. Find the parent of the newly added bookmark,
            // if it has a parent. 
            List<FileAndBookmarkWrapper> sorted = FileBookmarks.OrderBy(x => x.Bookmark.StartPage).ToList();
            FileAndBookmarkWrapper parent = FileAndBookmarkWrapper.FindParent(sorted, input.StartPage, input.EndPage);
            int level = parent == null ? 1 : parent.Bookmark.Level + 1;
            FileAndBookmarkWrapper addMark = new FileAndBookmarkWrapper(new LeveledBookmark(level, input.Title, 
                input.StartPage, input.EndPage - input.StartPage + 1), input.FilePath);

            FileAndBookmarkWrapper precedingSibling = addMark.FindPrecedingSibling(sorted, parent);
            IList<FileAndBookmarkWrapper> children = addMark.FindChildren(sorted);

            // Default as first bookmark
            int index = 0;
            // Sub-level bookmark, but first of its kind
            if (parent != null && precedingSibling == null)
                index = FileBookmarks.IndexOf(parent) + 1;
            // Not the first of its kind, top-level or sub-level
            if (precedingSibling != null)
                index = FileBookmarks.IndexOf(precedingSibling) + 1;

            addMark.IsSelected = true;
            FileBookmarks.Insert(index, addMark);
            foreach (FileAndBookmarkWrapper child in children)
            {
                int childIndex = FileBookmarks.IndexOf(child);
                FileBookmarks.RemoveAt(childIndex);
                FileBookmarks.Insert(childIndex, new FileAndBookmarkWrapper(
                    new LeveledBookmark(child.Bookmark.Level + 1, child.Bookmark.Title, child.Bookmark.Pages), SelectedFile.FilePath));
            }
        }

        private IAsyncCommand addCommand;
        public IAsyncCommand AddCommand =>
            addCommand ?? (addCommand = new AsyncCommand(ExecuteAddCommand));

        private async Task ExecuteAddCommand()
        {
            BookmarkDialog dialog = new BookmarkDialog(Resources.Labels.Dialogs.Bookmark.New)
            {
                StartPage = 1,
                EndPage = 1
            };

            await dialogAssist.Show(dialog);

            if (dialog.IsCanceled)
            {
                logbook.Write($"Cancellation requested at {nameof(IDialog)} '{dialog.DialogTitle}'.", LogLevel.Information);

                return;
            }

            BookmarkInfo info = new BookmarkInfo(dialog.StartPage, dialog.EndPage, dialog.Title, SelectedFile.FilePath);
            BookmarkAdded(info);

            logbook.Write($"{nameof(BookmarkInfo)} '{info.Title}' added.", LogLevel.Information);
        }

        private IAsyncCommand saveSeparateCommand;
        public IAsyncCommand SaveSeparateCommand =>
            saveSeparateCommand ?? (saveSeparateCommand = new AsyncCommand(ExecuteSaveSeparateCommand));

        private async Task ExecuteSaveSeparateCommand()
        {
            if (configuration.ExtractionCreateZip)
            {
                await SaveAsZip(false);
                return;
            }

            string path = Input.OpenDirectory(Resources.UserInput.Descriptions.SelectSaveFolder);
            if (path == null) return;

            DirectoryInfo destination = new DirectoryInfo(path);

            logbook.Write($"Starting extraction to directory.", LogLevel.Information);

            await executor.Save(destination, Files);

            logbook.Write($"Extraction finished.", LogLevel.Information);
        }

        private IAsyncCommand saveFileCommand;
        public IAsyncCommand SaveFileCommand =>
            saveFileCommand ?? (saveFileCommand = new AsyncCommand(ExecuteSaveFileCommand));

        private async Task ExecuteSaveFileCommand()
        {
            if (configuration.ExtractionCreateZip)
            {
                await SaveAsZip(true);
                return;
            }

            string path = Input.SaveFile(Resources.UserInput.Descriptions.SelectSaveFile, FileType.PDF,
                new DirectoryInfo(Path.GetDirectoryName(currentFilePath)));
            if (path == null) return;

            FileInfo destination = new FileInfo(path);

            logbook.Write($"Starting extraction to file.", LogLevel.Information);

            await executor.Save(destination, Files);

            logbook.Write($"Extraction finished.", LogLevel.Information);
        }

        private async Task SaveAsZip(bool saveSingular)
        {
            FileSystemInfo destination;
            DirectoryInfo tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
            tempDir.Create();

            if (saveSingular)
            {
                ExtractSettingsDialog fileNameDialog = new ExtractSettingsDialog(
                Resources.Labels.Dialogs.ExtractionOptions.ZipDialogTitle, true,
                Resources.Labels.Dialogs.ExtractionOptions.ZipName,
                Resources.Labels.Dialogs.ExtractionOptions.ZipNameHelper, false);

                await dialogAssist.Show(fileNameDialog);

                if (fileNameDialog.IsCanceled) return;

                string fileName = Path.GetFileNameWithoutExtension(fileNameDialog.Title.ReplaceIllegal()) + ".pdf";
                string fullPath = Path.Combine(tempDir.FullName, fileName);
                destination = new FileInfo(fullPath);
            }
            else
            {
                destination = tempDir;
            }

            string zipPath = Input.SaveFile(Resources.UserInput.Descriptions.SelectSaveFile, FileType.Zip,
                new DirectoryInfo(Path.GetDirectoryName(currentFilePath)));
            FileInfo zipFile = new FileInfo(zipPath);

            logbook.Write($"Starting extraction to zip-file.", LogLevel.Information);

            await executor.SaveAsZip(destination, Files, zipFile);
            await Task.Run(() => tempDir.Delete(true));

            logbook.Write($"Extraction finished.", LogLevel.Information);
        }

        private DelegateCommand<SelectionChangedEventArgs> selectionCommand;
        public DelegateCommand<SelectionChangedEventArgs> SelectionCommand =>
            selectionCommand ?? (selectionCommand = new DelegateCommand<SelectionChangedEventArgs>(ExecuteSelectionCommand));

        void ExecuteSelectionCommand(SelectionChangedEventArgs parameter)
        {
            if (parameter.AddedItems.Count > 0)
            {
                FileAndBookmarkWrapper wrapper = parameter.AddedItems[0] as FileAndBookmarkWrapper;

                SelectChildrenRecursively(wrapper);
                BookmarkInfo info = new BookmarkInfo(
                    wrapper.Bookmark.StartPage,
                    wrapper.Bookmark.EndPage,
                    wrapper.Bookmark.Title,
                    wrapper.FilePath,
                    wrapper.Id);

                eventAggregator.GetEvent<BookmarkSelectedEvent>().Publish(info);
            }
        }

        private void SelectChildrenRecursively(FileAndBookmarkWrapper mark)
        {
            foreach (FileAndBookmarkWrapper child in mark.FindChildren(FileBookmarks))
            {
                child.IsSelected = true;
                SelectChildrenRecursively(child);
            }
        }
        private void DeSelectChildrenRecursively(FileAndBookmarkWrapper mark, ObservableCollection<FileAndBookmarkWrapper> bookmarks)
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
        private void DeSelectParent(FileAndBookmarkWrapper mark, ObservableCollection<FileAndBookmarkWrapper> bookmarks)
        {
            FileAndBookmarkWrapper parent = mark.FindParent(bookmarks);
            if (parent != null)
            {
                parent.IsSelected = false;
            }
        }

        private DelegateCommand viewFileCommand;
        public DelegateCommand ViewFileCommand =>
            viewFileCommand ?? (viewFileCommand = new DelegateCommand(ExecuteViewFileCommand));

        void ExecuteViewFileCommand()
        {
            if (SelectedFile != null)
            {
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
        public DelegateCommand DeleteFileCommand =>
            deleteFileCommand ?? (deleteFileCommand = new DelegateCommand(ExecuteDeleteFileCommand));

        void ExecuteDeleteFileCommand()
        {
            if (SelectedFile != null)
            {
                int index = Files.IndexOf(SelectedFile);
                string path = SelectedFile.FilePath;
                Files.Remove(SelectedFile);
                eventAggregator.GetEvent<BookmarkFileDeletedEvent>().Publish(path);
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
    }
}
