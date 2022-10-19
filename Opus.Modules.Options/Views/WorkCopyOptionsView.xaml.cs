using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
