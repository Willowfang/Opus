using Opus.Core.Base;
using Opus.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Opus.Events.Data;
using Opus.Services.UI;
using Opus.Services.Implementation.UI.Dialogs;

namespace Opus.Modules.Options.ViewModels
{
    public class BookmarkNewViewModel : ViewModelBase
    {
        private IEventAggregator eventAggregator;
        private IDialogAssist dialogAssist;

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

        public BookmarkNewViewModel(IEventAggregator eventAggregator, IDialogAssist dialogAssist)
        {
            eventAggregator.GetEvent<FileSelectedEvent>().Subscribe(emptyBoxes);
            this.eventAggregator = eventAggregator;
            this.dialogAssist = dialogAssist;
        }

        private DelegateCommand addCommand;
        public DelegateCommand AddCommand =>
            addCommand ?? (addCommand = new DelegateCommand(ExecuteAddCommand));

        private void ExecuteAddCommand()
        {
            if (StartPage == 0 || EndPage == 0)
            {
                dialogAssist.Show(new MessageDialog(Resources.Messages.PageNumberZero));
            }
            else if (EndPage - StartPage < 0)
            {
                dialogAssist.Show(new MessageDialog(Resources.Messages.PageNumberNegative));
            }
            else if (string.IsNullOrWhiteSpace(Title))
            {
                dialogAssist.Show(new MessageDialog(Resources.Messages.BookmarkTitleNull));
            }
            else
            {
                BookmarkInfo info = new BookmarkInfo(StartPage, EndPage, Title);
                eventAggregator.GetEvent<BookmarkAddedEvent>().Publish(info);
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
