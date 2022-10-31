using System;
using System.Windows;
using System.Windows.Controls;

namespace Opus.Modules.Options.Views
{
    /// <summary>
    /// Work copy options code-behind.
    /// </summary>
    public partial class WorkCopyOptionsView : UserControl
    {
        /// <summary>
        /// Create work copy options view.
        /// </summary>
        public WorkCopyOptionsView()
        {
            InitializeComponent();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.Dispatcher.BeginInvoke(new Action(() => textBox.SelectAll()));
        }
    }
}
