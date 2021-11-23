using Opus.Core.Base;
using Opus.Core.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Opus.Core.Events.Data;

namespace Opus.Modules.Options.ViewModels
{
    public class BookmarkNewViewModel : ViewModelBase
    {
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

        public BookmarkNewViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
            : base (regionManager, eventAggregator)
        {
            eventAggregator.GetEvent<FileSelectedEvent>().Subscribe(emptyBoxes);
        }

        private DelegateCommand addCommand;
        public DelegateCommand AddCommand =>
            addCommand ?? (addCommand = new DelegateCommand(ExecuteAddCommand));

        void ExecuteAddCommand()
        {
            if (StartPage == 0 || EndPage == 0)
                Aggregator.GetEvent<DialogMessageEvent>().Publish(Resources.Messages.PageNumberZero);
            else if (EndPage - StartPage < 0)
                Aggregator.GetEvent<DialogMessageEvent>().Publish(Resources.Messages.PageNumberNegative);
            else if (string.IsNullOrWhiteSpace(Title))
                Aggregator.GetEvent<DialogMessageEvent>().Publish(Resources.Messages.BookmarkTitleNull);
            else
            {
                BookmarkInfo info = new BookmarkInfo(StartPage, EndPage, Title);
                Aggregator.GetEvent<BookmarkAddedEvent>().Publish(info);
                emptyBoxes(null);
            }
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
