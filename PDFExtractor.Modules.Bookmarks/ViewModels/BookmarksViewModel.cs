using Prism.Mvvm;
using System.Collections.ObjectModel;
using ExtLib;
using Prism.Events;
using PDFExtractor.Core.Events;
using Prism.Commands;
using System.Windows;
using PDFExtractor.Core.ExtensionMethods;
using System.Windows.Data;
using System.ComponentModel;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Win32;
using System.IO;
using System;
using System.Diagnostics;

namespace PDFExtractor.Modules.Bookmarks.ViewModels
{
    public class BookmarksViewModel : BindableBase
    {
        public ObservableCollection<IBookmark> FileBookmarks { get; set; }

        private IBookmark selectedBookmark;
        public IBookmark SelectedBookmark
        {
            get { return selectedBookmark; }
            set { SetProperty(ref selectedBookmark, value); }
        }

        private string FilePath;

        public BookmarksViewModel(IEventAggregator eventAggregator)
        {
            eventAggregator.GetEvent<FileSelectedEvent>().Subscribe(FileSelected);
            eventAggregator.GetEvent<BookmarkAddedEvent>().Subscribe(BookmarkAdded);
            FileBookmarks = new ObservableCollection<IBookmark>();
        }

        private void BookmarkAdded(IBookmark mark)
        {
            List<IBookmark> originalList = FileBookmarks.ToList();

            List<IBookmark> parents =
                originalList.FindAll(x => x.StartPage <= mark.StartPage && x.EndPage > mark.EndPage);
            if (parents.Count > 0)
            {
                IBookmark parent = parents.First(x => x.Level == parents.Max(x => x.Level));
                mark.Level = parent.Level + 1;
                mark.ParentId = parent.Id;

                List<IBookmark> siblings = originalList.FindAll(x => x.ParentId == parent.Id);
                if (siblings.Count < 1)
                {
                    FileBookmarks.Insert(FileBookmarks.IndexOf(FileBookmarks.First(x => x.Id == parent.Id)) + 1, mark);
                }
                else
                {
                    IBookmark closestSibling = siblings.FirstOrDefault(x => x.StartPage > mark.StartPage);
                    if (closestSibling == null)
                    {
                        FileBookmarks.Insert(FileBookmarks.IndexOf(FileBookmarks.Last(x => x.ParentId == parent.Id)) + 1, mark);
                    }
                    else
                    {
                        FileBookmarks.Insert(FileBookmarks.IndexOf(FileBookmarks.First(x => x.Id == closestSibling.Id)) - 1, mark);
                    }
                }
            }
            else
            {
                mark.ParentId = Guid.Empty;
                mark.Level = 1;
                IBookmark next = FileBookmarks.FirstOrDefault(x => x.Level == 0 && x.StartPage > mark.StartPage);
                if (next == null)
                    FileBookmarks.Add(mark);
                else
                    FileBookmarks.Insert(FileBookmarks.IndexOf(next) - 1, mark);
            }
        }

        private void FileSelected(string filePath)
        {
            FileBookmarks.Clear();
            FilePath = filePath;
            ExtLib.Bookmarks.GetBookmarks(filePath).ForEach(x => FileBookmarks.Add(x));
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

        private DelegateCommand saveFileCommand;
        public DelegateCommand SaveFileCommand =>
            saveFileCommand ?? (saveFileCommand = new DelegateCommand(ExecuteSaveFileCommand));

        void ExecuteSaveFileCommand()
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Title = "Valitse tallennettavan tiedoston polku";
            saveDialog.Filter = "PDF (.pdf)|*.pdf";
            saveDialog.InitialDirectory = Path.GetDirectoryName(FilePath);

            if (saveDialog.ShowDialog() != true)
                return;

            Extraction.Extract(FilePath, saveDialog.FileName, FileBookmarks);
            SelectedBookmark = null;
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(@saveDialog.FileName)
            {
                UseShellExecute = true
            };
            p.Start();
        }
    }
}
