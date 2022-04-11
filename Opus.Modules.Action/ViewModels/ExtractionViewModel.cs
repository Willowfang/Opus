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

namespace Opus.Modules.Action.ViewModels
{
    public class ExtractionViewModel : ViewModelBaseLogging<ExtractionViewModel>, INavigationTarget
    {
        private IExtractionExecutor executor;
        private IPathSelection Input;
        private IDialogAssist dialogAssist;
        private IEventAggregator eventAggregator;
        private IBookmarkService bookmarkService;
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
            }
        }

        private ObservableCollection<BookmarkStorage> fileBookmarks;
        public ObservableCollection<BookmarkStorage> FileBookmarks
        {
            get => fileBookmarks;
            set => SetProperty(ref fileBookmarks, value);
        }

        private BookmarkStorage selectedBookmark;
        public BookmarkStorage SelectedBookmark
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

        public ExtractionViewModel(
            IEventAggregator eventAggregator,
            IPathSelection input, 
            IDialogAssist dialogAssist,
            INavigationTargetRegistry navregistry, 
            IExtractionExecutor executor,
            IBookmarkService bookmarkService,
            ILogbook logbook) : base(logbook)
        {
            this.eventAggregator = eventAggregator;
            Files = new ObservableCollection<FileAndBookmarksStorage>();
            FileBookmarks = new ObservableCollection<BookmarkStorage>();
            Input = input;
            this.dialogAssist = dialogAssist;
            this.executor = executor;
            this.bookmarkService = bookmarkService;
            navregistry.AddTarget(SchemeNames.SPLIT, this);
        }

        SubscriptionToken filesAddedSubscription;
        public void OnArrival()
        {
            filesAddedSubscription = eventAggregator.GetEvent<FilesAddedEvent>().Subscribe(FilesAdded);

            logbook.Write($"{this} subscribed to {nameof(FilesAddedEvent)}.", LogLevel.Debug);
        }
        public void WhenLeaving()
        {
            eventAggregator.GetEvent<FilesAddedEvent>().Unsubscribe(filesAddedSubscription);

            logbook.Write($"{this} unsubscribed from {nameof(FilesAddedEvent)}.", LogLevel.Debug);
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
                        storage.Bookmarks.Add(new BookmarkStorage(found));
                    }
                    Files.Add(storage);
                }
            }

            if (SelectedFile == null)
                SelectedFile = Files.FirstOrDefault();

            IsFileSelected = true;
            currentFilePath = filePaths.LastOrDefault();
        }

        private void BookmarkAdded(BookmarkInfo input)
        {
            // Sort the bookmarks in order in a new list. Find the parent of the newly added bookmark,
            // if it has a parent. 
            List<BookmarkStorage> sorted = FileBookmarks.OrderBy(x => x.Value.StartPage).ToList();
            BookmarkStorage parent = FindParent(sorted, input.StartPage, input.EndPage);
            int level = parent == null ? 1 : parent.Value.Level + 1;
            BookmarkStorage addMark = new BookmarkStorage(new LeveledBookmark(level, input.Title, 
                input.StartPage, input.EndPage - input.StartPage + 1));

            BookmarkStorage precedingSibling = FindPrecedingSibling(sorted, addMark, parent);
            IList<BookmarkStorage> children = FindChildren(sorted, addMark);

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
            foreach (BookmarkStorage child in children)
            {
                int childIndex = FileBookmarks.IndexOf(child);
                FileBookmarks.RemoveAt(childIndex);
                FileBookmarks.Insert(childIndex, new BookmarkStorage(
                    new LeveledBookmark(child.Value.Level + 1, child.Value.Title, child.Value.Pages)));
            }
            SelectChildrenRecursively(addMark);

        }

        private DelegateCommand clearCommand;
        public DelegateCommand ClearCommand =>
            clearCommand ?? (clearCommand = new DelegateCommand(ExecuteClearCommand));

        void ExecuteClearCommand()
        {
            SelectedBookmark = null;
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

            BookmarkInfo info = new BookmarkInfo(dialog.StartPage, dialog.EndPage, dialog.Title);
            BookmarkAdded(info);

            logbook.Write($"{nameof(BookmarkInfo)} '{info.Title}' added.", LogLevel.Information);
        }

        private IAsyncCommand editCommand;
        public IAsyncCommand EditCommand =>
            editCommand ??= new AsyncCommand(ExecuteEditCommand);

        private async Task ExecuteEditCommand()
        {
            if (SelectedBookmark == null)
            {
                return;
            }

            BookmarkDialog dialog = new BookmarkDialog(Resources.Labels.Dialogs.Bookmark.Edit)
            {
                Title = SelectedBookmark.Value.Title,
                StartPage = SelectedBookmark.Value.StartPage,
                EndPage = SelectedBookmark.Value.EndPage
            };

            await dialogAssist.Show(dialog);

            if (dialog.IsCanceled)
            {
                logbook.Write($"Cancellation requested at {nameof(IDialog)} '{dialog}'.", LogLevel.Information);
                return;
            }

            BookmarkInfo edited = new BookmarkInfo(dialog.StartPage, dialog.EndPage, dialog.Title);
            FileBookmarks.Remove(SelectedBookmark);
            BookmarkAdded(edited);

            logbook.Write($"{nameof(BookmarkInfo)} '{edited.Title}' edited.", LogLevel.Information);
        }

        private DelegateCommand deleteCommand;
        public DelegateCommand DeleteCommand =>
            deleteCommand ?? (deleteCommand = new DelegateCommand(ExecuteDeleteCommand));

        void ExecuteDeleteCommand()
        {
            FileBookmarks.RemoveAll(x => x.IsSelected);
        }

        private IAsyncCommand saveSeparateCommand;
        public IAsyncCommand SaveSeparateCommand =>
            saveSeparateCommand ?? (saveSeparateCommand = new AsyncCommand(ExecuteSaveSeparateCommand));

        private async Task ExecuteSaveSeparateCommand()
        {
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
            string path = Input.SaveFile(Resources.UserInput.Descriptions.SelectSaveFile, FileType.PDF,
                new DirectoryInfo(Path.GetDirectoryName(currentFilePath)));
            if (path == null) return;

            FileInfo destination = new FileInfo(path);

            logbook.Write($"Starting extraction to file.", LogLevel.Information);

            await executor.Save(destination, Files);

            logbook.Write($"Extraction finished.", LogLevel.Information);
        }

        private DelegateCommand<SelectionChangedEventArgs> selectChildrenCommand;
        public DelegateCommand<SelectionChangedEventArgs> SelectChildrenCommand =>
            selectChildrenCommand ?? (selectChildrenCommand = new DelegateCommand<SelectionChangedEventArgs>(ExecuteSelectChildrenCommand));

        void ExecuteSelectChildrenCommand(SelectionChangedEventArgs parameter)
        {
            if (parameter.AddedItems.Count > 0)
                SelectChildrenRecursively(parameter.AddedItems[0] as BookmarkStorage);
            if (parameter.RemovedItems.Count > 0)
            {
                DeSelectChildrenRecursively(parameter.RemovedItems[0] as BookmarkStorage);
                DeSelectParent(parameter.RemovedItems[0] as BookmarkStorage);
            }
        }

        private void SelectChildrenRecursively(BookmarkStorage mark)
        {
            foreach (BookmarkStorage child in FindChildren(FileBookmarks, mark))
            {
                child.IsSelected = true;
                SelectChildrenRecursively(child);
            }
        }
        private void DeSelectChildrenRecursively(BookmarkStorage mark)
        {
            if (mark is null || FileBookmarks is null)
                return;

            IList<BookmarkStorage> children = FindChildren(FileBookmarks, mark);
            if (children.All(c => c.IsSelected))
            {
                foreach (BookmarkStorage child in FindChildren(FileBookmarks, mark))
                {
                    child.IsSelected = false;
                    DeSelectChildrenRecursively(child);
                }
            }
        }
        private void DeSelectParent(BookmarkStorage mark)
        {
            BookmarkStorage parent = FindParent(FileBookmarks, mark.Value.StartPage, mark.Value.EndPage);
            if (parent != null)
            {
                parent.IsSelected = false;
            }
        }

        private DelegateCommand selectAllCommand;
        public DelegateCommand SelectAllCommand =>
            selectAllCommand ?? (selectAllCommand = new DelegateCommand(ExecuteSelectAllCommand));

        void ExecuteSelectAllCommand()
        {
            foreach (BookmarkStorage mark in FileBookmarks)
            {
                mark.IsSelected = true;
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
                Files.Remove(SelectedFile);
                if (Files.Count > 0)
                {
                    SelectedFile = Files[index - 1];
                }
                else
                {
                    SelectedFile = null;
                }
            }
        }

        private BookmarkStorage FindParent(IList<BookmarkStorage> storage, int childStartPage,
            int childEndPage)
        {
            if (storage is null)
                return null;

            return storage.LastOrDefault(x =>
                (x.Value.StartPage < childStartPage && x.Value.EndPage == childEndPage) ||
                (x.Value.StartPage <= childStartPage && x.Value.EndPage > childEndPage));
        }
        private BookmarkStorage FindPrecedingSibling(IList<BookmarkStorage> storage,
            BookmarkStorage current, BookmarkStorage commonParent)
        {
            if (storage is null || current is null)
                return null;

            if (commonParent == null)
                return storage.LastOrDefault(x =>
                    x.Value.Level == 1 &&
                    x.Value.StartPage <= current.Value.StartPage &&
                    x.Value.EndPage < current.Value.EndPage);

            return storage.LastOrDefault(x =>
                x.Value.StartPage >= commonParent.Value.StartPage &&
                x.Value.EndPage <= commonParent.Value.EndPage &&
                x.Value.StartPage < current.Value.StartPage);
        }
        private IList<BookmarkStorage> FindChildren(IList<BookmarkStorage> storage,
            BookmarkStorage parent)
        {
            return storage.Where(x =>
                (x.Value.StartPage > parent.Value.StartPage && x.Value.EndPage == parent.Value.EndPage) ||
                (x.Value.StartPage >= parent.Value.StartPage && x.Value.EndPage < parent.Value.EndPage)).ToList();
        }
    }
}
