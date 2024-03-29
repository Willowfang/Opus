﻿using System.Windows.Controls;
using System.Windows.Input;

namespace Opus.Common.Controls
{
    /// <summary>
    /// List for reordering bookmarks.
    /// </summary>
    public partial class OrderList : UserControl
    {
        /// <summary>
        /// Create a new list.
        /// </summary>
        public OrderList()
        {
            InitializeComponent();
        }

        private void ListBoxItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is ListBoxItem item)
            {
                item.IsSelected = true;
            }
        }
    }
}
