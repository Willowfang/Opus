using Opus.Services.UI;
using Prism.Commands;
using Prism.Mvvm;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Opus.Services.Implementation.UI
{
    public abstract class DialogBase : BindableBase, IDialog
    {
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

        public DialogBase()
        {
            SaveCommand = new DelegateCommand(ExecuteSave, SaveCanExecute);
            CloseCommand = new DelegateCommand(ExecuteClose, CloseCanExecute);
            DialogClosed = new TaskCompletionSource();
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
