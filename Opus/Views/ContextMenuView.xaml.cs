using Opus.ViewModels;
using System.Windows;

namespace Opus.Views
{
    /// <summary>
    /// View for Context menu operation. Contains a content control for displaying various
    /// dialog content. Otherwise the view is empty. Corresponding ViewModel is <see cref="ContextMenuViewModel"/>.
    /// </summary>
    public partial class ContextMenuView : Window
    {
        /// <summary>
        /// Create a new Context menu view.
        /// </summary>
        public ContextMenuView()
        {
            InitializeComponent();
        }
    }
}
