using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Opus.Styles.Controls
{
    /// <summary>
    /// Interaction logic for OrderList.xaml
    /// </summary>
    public partial class OrderList : UserControl
    {
        public OrderList()
        {
            InitializeComponent();
        }

        private void ListBoxItem_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is ListBoxItem item)
            {
                item.IsSelected = true;
            }
        }
    }
}
