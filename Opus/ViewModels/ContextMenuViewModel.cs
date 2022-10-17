using Opus.Services.UI;
using Prism.Mvvm;

namespace Opus.ViewModels
{
    /// <summary>
    /// Handles UX when starting the application with parameters
    /// (either from the command line of from context menu). Full
    /// GUI is only displayed when the program is started without
    /// parameters.
    /// </summary>
    public class ContextMenuViewModel : BindableBase
    {
        /// <summary>
        /// Handles the dialogs shown to the user.
        /// </summary>
        public IDialogAssist Dialog { get; }

        /// <summary>
        /// Is responsible for handling the request and
        /// parameters for an operation.
        /// </summary>
        public IContextMenu ContextMenu { get; }

        public ContextMenuViewModel(IDialogAssist dialog, IContextMenu contextMenu)
        {
            Dialog = dialog;
            ContextMenu = contextMenu;
        }
    }
}
