using ExtLib;
using PDFExtractor.Core.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFExtractor.Modules.NewBookmark.ViewModels
{
    public class BookmarkNewViewModel : BindableBase
    {
        private IEventAggregator aggregator;

        private int startPage;
        public int StartPage
        {
            get { return startPage; }
            set { SetProperty(ref startPage, value); }
        }
        private int endPage;
        public int EndPage
        {
            get { return endPage; }
            set { SetProperty(ref endPage, value); }
        }
        private string title;
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        public BookmarkNewViewModel(IEventAggregator eventAggregator)
        {
            aggregator = eventAggregator;
            eventAggregator.GetEvent<FileSelectedEvent>().Subscribe(emptyBoxes);
        }

        private DelegateCommand addCommand;
        public DelegateCommand AddCommand =>
            addCommand ?? (addCommand = new DelegateCommand(ExecuteAddCommand));

        void ExecuteAddCommand()
        {
            Bookmarks.Bookmark mark =
                new Bookmarks.Bookmark(Title, StartPage);
            mark.EndPage = EndPage;
            mark.IsSelected = true;

            aggregator.GetEvent<BookmarkAddedEvent>().Publish(mark);

            emptyBoxes(null);
        }

        private DelegateCommand clearNameCommand;
        public DelegateCommand ClearNameCommand =>
            clearNameCommand ?? (clearNameCommand = new DelegateCommand(ExecuteClearNameCommand));

        void ExecuteClearNameCommand()
        {
            emptyBoxes(null);
        }

        private void emptyBoxes(string parameter)
        {
            StartPage = 0;
            EndPage = 0;
            Title = null;
        }
    }
}
