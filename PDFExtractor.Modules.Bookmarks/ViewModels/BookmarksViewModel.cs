using Prism.Mvvm;
using System.Collections.ObjectModel;
using ExtLib;
using Prism.Events;
using PDFExtractor.Core.Events;
using Prism.Commands;
using System.Windows;
using System.Windows.Forms;
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
using System.Windows.Controls;

namespace PDFExtractor.Modules.Bookmarks.ViewModels
{
    public class BookmarksViewModel : BindableBase
    {
        private IEventAggregator aggregator;
        private int PageNumber;
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
            aggregator = eventAggregator;
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
            PageNumber = ExtLib.Bookmarks.GetLastPage(filePath);
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

        private DelegateCommand saveSeparateCommand;
        public DelegateCommand SaveSeparateCommand =>
            saveSeparateCommand ?? (saveSeparateCommand = new DelegateCommand(ExecuteSaveSeparateCommand));

        void ExecuteSaveSeparateCommand()
        {
            FolderBrowserDialog browseDialog = new FolderBrowserDialog();
            browseDialog.Description = "Valitse kansio, johon tiedostot tallennetaan";
            browseDialog.UseDescriptionForTitle = true;
            browseDialog.ShowNewFolderButton = true;

            if (browseDialog.ShowDialog() == DialogResult.Cancel)
                return;

            Extraction.ExtractSeparate(FilePath, browseDialog.SelectedPath, FileBookmarks);
            SelectedBookmark = null;
            aggregator.GetEvent<DialogEvent>().Publish("Tiedostot tallennettu!");
        }

        private DelegateCommand saveFileCommand;
        public DelegateCommand SaveFileCommand =>
            saveFileCommand ?? (saveFileCommand = new DelegateCommand(ExecuteSaveFileCommand));

        void ExecuteSaveFileCommand()
        {
            Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog();
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

        private DelegateCommand importBookmarksCommand;
        public DelegateCommand ImportBookmarksCommand =>
            importBookmarksCommand ?? (importBookmarksCommand = new DelegateCommand(ExecuteImportBookmarksCommand));

        void ExecuteImportBookmarksCommand()
        {
            Microsoft.Win32.OpenFileDialog openFile = new Microsoft.Win32.OpenFileDialog();
            openFile.Title = "Valitse kirjanmerkkitiedosto";
            openFile.Filter = "Tekstitiedosto |*.txt";
            openFile.InitialDirectory = Path.GetDirectoryName(FilePath);

            if (openFile.ShowDialog() != true)
                return;

            List<IBookmark> imported = ExtLib.Bookmarks.ImportBookmarks(openFile.FileName, PageNumber);
            FileBookmarks.Clear();
            foreach (IBookmark import in imported)
                FileBookmarks.Add(import);
        }

        private DelegateCommand<SelectionChangedEventArgs> selectChildrenCommand;
        public DelegateCommand<SelectionChangedEventArgs> SelectChildrenCommand =>
            selectChildrenCommand ?? (selectChildrenCommand = new DelegateCommand<SelectionChangedEventArgs>(ExecuteSelectChildrenCommand));

        void ExecuteSelectChildrenCommand(SelectionChangedEventArgs parameter)
        {
            if (parameter.AddedItems.Count > 0)
                SelectChildrenRecursively(parameter.AddedItems[0] as IBookmark);
            if (parameter.RemovedItems.Count > 0)
                DeSelectChildrenRecursively(parameter.RemovedItems[0] as IBookmark);
        }

        private void SelectChildrenRecursively(IBookmark mark)
        {
            foreach (IBookmark child in FileBookmarks.Where(x => x.ParentId == mark.Id))
            {
                child.IsSelected = true;
                SelectChildrenRecursively(child);
            }
        }
        private void DeSelectChildrenRecursively(IBookmark mark)
        {
            foreach (IBookmark child in FileBookmarks.Where(x => x.ParentId == mark.Id))
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
            foreach (IBookmark mark in FileBookmarks)
            {
                mark.IsSelected = true;
            }
        }
    }
}
