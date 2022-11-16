using Opus.Common.Wrappers;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Opus.Modules.Action.Views
{
    /// <summary>
    /// Extraction view code-behind.
    /// </summary>
    public partial class ExtractionView : UserControl
    {
        /// <summary>
        /// Create extraction view.
        /// </summary>
        public ExtractionView()
        {
            InitializeComponent();
        }

        private void ListViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            FileAndBookmarkWrapper wrapper = element.DataContext as FileAndBookmarkWrapper;
            if (wrapper.IsSelected)
                element.Visibility = Visibility.Collapsed;
        }

        private void ListViewItem_Selected(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            DoubleAnimation animation = new DoubleAnimation(
                0,
                TimeSpan.FromMilliseconds(300),
                FillBehavior.Stop
            );
            animation.Completed += (s, e) =>
            {
                element.Opacity = 0;
                element.Visibility = Visibility.Collapsed;
            };
            element.BeginAnimation(OpacityProperty, animation);
        }

        private void ListViewItem_Unselected(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            DoubleAnimation animation = new DoubleAnimation(
                1,
                TimeSpan.FromMilliseconds(300),
                FillBehavior.Stop
            );
            animation.Completed += (s, e) =>
            {
                element.Opacity = 1;
            };
            element.Visibility = Visibility.Visible;
            element.BeginAnimation(OpacityProperty, animation);
        }
    }
}
