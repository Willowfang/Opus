using Opus.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Opus.Views
{
    /// <summary>
    /// Main window code behing. The viewModel associated with main window view is
    /// <see cref="Opus.ViewModels.MainWindowViewModel"/>. This partial class only contains
    /// members and event listeners that are directly related to UI, in accordance with MVVM.
    /// </summary>
    public partial class MainWindowView : Window
    {
        /// <summary>
        /// Create a new main window.
        /// </summary>
        public MainWindowView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// When the window is loaded, select Extraction as the default view and mark
        /// it's button selected.
        /// </summary>
        /// <param name="sender">The sending object (current window).</param>
        /// <param name="e">Event arguments.</param>
        protected void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SplitButton.IsChecked = true;
            SplitButton.Command.Execute(SchemeNames.EXTRACT);
        }

        /// <summary>
        /// The window can be moved by dragging the top bar.
        /// </summary>
        /// <param name="sender">Sending bar.</param>
        /// <param name="e">Event arguments.</param>
        protected void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState != WindowState.Maximized)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        }
    }
}
