using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WPFSample.Panes
{
    /// <summary>
    /// HistogramSample.xaml 的交互逻辑
    /// </summary>
    public partial class HistogramSample : UserControl
    {
        public HistogramSample()
        {
            InitializeComponent();
        }

        private void Photo_OnMouseEnter(object sender, MouseEventArgs e)
        {
            var border = (Border) sender;
            border.BorderBrush = new SolidColorBrush(Colors.White);
            border.BorderThickness = new Thickness(2);
            Cursor = Cursors.Hand;
        }

        private void Photo_OnMouseLeave(object sender, MouseEventArgs e)
        {
            var border = (Border)sender;
            border.BorderBrush = new SolidColorBrush(Colors.Transparent);
            border.BorderThickness = new Thickness(2);
            Cursor = Cursors.Arrow;
        }
    }
}
