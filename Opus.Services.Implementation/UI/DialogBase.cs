using Opus.Services.UI;
using Prism.Commands;
using Prism.Mvvm;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Opus.Services.Implementation.UI
{
    public abstract class DialogBase : BindableBase, IDialog
    {
        public string DialogTitle { get; protected set; }

        private bool isCanceled;
        public bool IsCanceled
        {
            get => isCanceled;
            set => SetProperty(ref isCanceled, value);
        }
        public TaskCompletionSource DialogClosed { get; set; }

        protected DelegateCommand SaveCommand;
        public ICommand Save => SaveCommand;
        protected DelegateCommand CloseCommand;
        public ICommand Close => CloseCommand;

        public DialogBase(string dialogTitle)
        {
            SaveCommand = new DelegateCommand(ExecuteSave, SaveCanExecute);
            CloseCommand = new DelegateCommand(ExecuteClose, CloseCanExecute);
            DialogClosed = new TaskCompletionSource();
            DialogTitle = dialogTitle;
        }
        protected virtual void ExecuteSave() 
        {
            DialogClosed.SetResult();
        }
        protected virtual bool SaveCanExecute()
        {
            return true;
        }
        protected virtual void ExecuteClose()
        {
            IsCanceled = true;
            DialogClosed.SetResult();
        }
        protected virtual bool CloseCanExecute()
        {
            return true;
        }
    }
}
