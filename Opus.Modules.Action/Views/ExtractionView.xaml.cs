using Opus.Services.Implementation.Data.Extraction;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Opus.Modules.Action.Views
{
    /// <summary>
    /// Interaction logic for ViewA.xaml
    /// </summary>
    public partial class ExtractionView : UserControl
    {
        public ExtractionView()
        {
            InitializeComponent();
        }

        private void ListViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            FileAndBookmarkWrapper wrapper = element.DataContext as FileAndBookmarkWrapper;
            if (wrapper.IsSelected) element.Visibility = Visibility.Collapsed;
        }

        private void ListViewItem_Selected(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            DoubleAnimation animation = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300), FillBehavior.Stop);
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
            DoubleAnimation animation = new DoubleAnimation(1, TimeSpan.FromMilliseconds(300), FillBehavior.Stop);
            animation.Completed += (s, e) =>
            {
                element.Opacity = 1;
            };
            element.Visibility = Visibility.Visible;
            element.BeginAnimation(OpacityProperty, animation);
        }
    }
}
