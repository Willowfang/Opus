using CX.LoggingLib;
using Opus.Services.Implementation.Logging;
using Opus.Services.UI;
using Prism.Commands;
using Prism.Mvvm;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Opus.Services.Implementation.UI
{
    /// <summary>
    /// Abstract base class for all dialogs.
    /// </summary>
    public abstract class DialogBase : BindableBase, IDialog
    {
        /// <summary>
        /// Title of the dialog.
        /// </summary>
        public string DialogTitle { get; protected set; }

        private bool isCanceled;

        /// <summary>
        /// If true, the dialog was cancelled by the user.
        /// </summary>
        public bool IsCanceled
        {
            get => isCanceled;
            set => SetProperty(ref isCanceled, value);
        }

        /// <summary>
        /// Command for closing the dialog.
        /// </summary>
        public TaskCompletionSource DialogClosed { get; set; }

        /// <summary>
        /// Backing property for save command.
        /// </summary>
        protected DelegateCommand SaveCommand;

        /// <summary>
        /// Save the dialog values.
        /// </summary>
        public ICommand Save => SaveCommand;

        /// <summary>
        /// Backing property for close command.
        /// </summary>
        protected DelegateCommand CloseCommand;

        /// <summary>
        /// Command for closing the dialog.
        /// </summary>
        public ICommand Close => CloseCommand;

        /// <summary>
        /// Create a new base dialog instance.
        /// </summary>
        /// <param name="dialogTitle">Title for the dialog.</param>
        public DialogBase(string dialogTitle)
        {
            SaveCommand = new DelegateCommand(ExecuteSave, SaveCanExecute);
            CloseCommand = new DelegateCommand(ExecuteClose, CloseCanExecute);
            DialogClosed = new TaskCompletionSource();
            DialogTitle = dialogTitle;
        }

        /// <summary>
        /// Method for executing save.
        /// </summary>
        protected virtual void ExecuteSave()
        {
            DialogClosed.SetResult();
        }

        /// <summary>
        /// Method for evaluating whether save is possible.
        /// </summary>
        /// <returns></returns>
        protected virtual bool SaveCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method for executing close (without saving).
        /// </summary>
        protected virtual void ExecuteClose()
        {
            IsCanceled = true;
            DialogClosed.SetResult();
        }

        /// <summary>
        /// Evaluation whether close can execute.
        /// </summary>
        /// <returns></returns>
        protected virtual bool CloseCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to perform when closing on error.
        /// </summary>
        public virtual void CloseOnError()
        {
            ExecuteClose();
        }
    }
}
