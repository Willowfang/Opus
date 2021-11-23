using System.Collections.ObjectModel;
using Prism.Events;
using Opus.Core.Events;
using Prism.Commands;
using System.Windows.Forms;
using Opus.Core.ExtensionMethods;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System;
using System.Windows.Controls;
using Opus.Core.Base;
using Prism.Regions;
using CX.PdfLib.Services;
using CX.PdfLib.Services.Data;
using CX.PdfLib.Common;
using Opus.Core.Wrappers;
using Opus.Core.Events.Data;
using System.Threading.Tasks;
using AsyncAwaitBestPractices.MVVM;
using Opus.Core.Constants;
using System.Threading;
using Opus.Core.Dialog;

namespace Opus.Modules.Action.ViewModels
{
    public class BookmarksViewModel : ViewModelBase
    {
        private IManipulator Manipulator;

        public ObservableCollection<BookmarkStorage> FileBookmarks { get; set; }
        private BookmarkStorage selectedBookmark;
        public BookmarkStorage SelectedBookmark
        {
            get { return selectedBookmark; }
            set { SetProperty(ref selectedBookmark, value); }
        }

        private string FilePath;

        public BookmarksViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, 
            IManipulator manipulator)
            : base(regionManager, eventAggregator)
        {
            eventAggregator.GetEvent<FileSelectedEvent>().Subscribe(FileSelected);
            eventAggregator.GetEvent<BookmarkAddedEvent>().Subscribe(BookmarkAdded);
            FileBookmarks = new ObservableCollection<BookmarkStorage>();
            Manipulator = manipulator;
        }

        private void BookmarkAdded(BookmarkInfo input)
        {
            // Sort the bookmarks in order in a new list. Find the parent of the newly added bookmark,
            // if it has a parent. 
            List<BookmarkStorage> sorted = FileBookmarks.OrderBy(x => x.Value.StartPage).ToList();
            BookmarkStorage parent = sorted.FindParent(input.StartPage, input.EndPage);
            int level = parent == null ? 1 : parent.Value.Level + 1;
            BookmarkStorage addMark = new BookmarkStorage(new LeveledBookmark(level, input.Title, 
                input.StartPage, input.EndPage - input.StartPage + 1));

            BookmarkStorage precedingSibling = sorted.FindPrecedingSibling(addMark, parent);
            IList<BookmarkStorage> children = sorted.FindChildren(addMark);

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

        private async void FileSelected(string filePath)
        {
            FileBookmarks.Clear();
            FilePath = filePath;
            foreach (ILeveledBookmark found in await Manipulator.FindBookmarksAsync(filePath))
            {
                FileBookmarks.Add(new BookmarkStorage(found));
            }
        }

        private DelegateCommand clearCommand;
        public DelegateCommand ClearCommand =>
            clearCommand ?? (clearCommand = new DelegateCommand(ExecuteClearCommand));

        void ExecuteClearCommand()
        {
            SelectedBookmark = null;
            
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
            FolderBrowserDialog browseDialog = new FolderBrowserDialog();
            browseDialog.Description = Resources.Labels.Bookmarks_SelectFolder;
            browseDialog.UseDescriptionForTitle = true;
            browseDialog.ShowNewFolderButton = true;

            if (browseDialog.ShowDialog() == DialogResult.Cancel)
                return;

            await Manipulator.ExtractAsync(FilePath, new DirectoryInfo(browseDialog.SelectedPath),
                FileBookmarks.Where(x => x.IsSelected).Select(y => y.Value), ShowProgress());
            SelectedBookmark = null;
        }

        private IAsyncCommand saveFileCommand;
        public IAsyncCommand SaveFileCommand =>
            saveFileCommand ?? (saveFileCommand = new AsyncCommand(ExecuteSaveFileCommand));

        private async Task ExecuteSaveFileCommand()
        {
            Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog();
            saveDialog.Title = Resources.Labels.Bookmarks_SelectPath;
            saveDialog.Filter = "PDF (.pdf)|*.pdf";
            saveDialog.InitialDirectory = Path.GetDirectoryName(FilePath);

            if (saveDialog.ShowDialog() != true)
                return;

            await Manipulator.ExtractAsync(FilePath, new FileInfo(saveDialog.FileName),
                FileBookmarks.Where(x => x.IsSelected).Select(y => y.Value), ShowProgress());
            SelectedBookmark = null;
        }

        private DelegateCommand<SelectionChangedEventArgs> selectChildrenCommand;
        public DelegateCommand<SelectionChangedEventArgs> SelectChildrenCommand =>
            selectChildrenCommand ?? (selectChildrenCommand = new DelegateCommand<SelectionChangedEventArgs>(ExecuteSelectChildrenCommand));

        void ExecuteSelectChildrenCommand(SelectionChangedEventArgs parameter)
        {
            if (parameter.AddedItems.Count > 0)
                SelectChildrenRecursively(parameter.AddedItems[0] as BookmarkStorage);
            if (parameter.RemovedItems.Count > 0)
                DeSelectChildrenRecursively(parameter.RemovedItems[0] as BookmarkStorage);
        }

        private void SelectChildrenRecursively(BookmarkStorage mark)
        {
            foreach (BookmarkStorage child in FileBookmarks.FindChildren(mark))
            {
                child.IsSelected = true;
                SelectChildrenRecursively(child);
            }
        }
        private void DeSelectChildrenRecursively(BookmarkStorage mark)
        {
            foreach (BookmarkStorage child in FileBookmarks.FindChildren(mark))
            {
                child.IsSelected = false;
                DeSelectChildrenRecursively(child);
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

        private IProgress<ProgressReport> ShowProgress()
        {
            Aggregator.GetEvent<ShowDialogEvent>().Publish(new ProgressDialog(0, ProgressPhase.Unassigned.GetResourceString()));
            return new Progress<ProgressReport>(report =>
            {
                Aggregator.GetEvent<ShowDialogEvent>().Publish(
                    new ProgressDialog(report.Percentage, report.CurrentPhase.GetResourceString(),
                    report.CurrentItem));
            });
        }
    }
}
