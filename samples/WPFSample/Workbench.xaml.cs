using System.Windows;
using Fluent;

namespace WPFSample
{
    /// <summary>
    /// Interaction logic for Workbench.xaml
    /// </summary>
    public partial class Workbench : RibbonWindow
    {
        public Workbench()
        {
            InitializeComponent();
        }


        private void Button1Base_OnClick(object sender, RoutedEventArgs e)
        {
            var d = new ProgressDialog();
            d.ShowDialog();
        }

        private void Button2Base_OnClick(object sender, RoutedEventArgs e)
        {
            var d = new ProgressDialog();
            d.Show();
        }
    }
}
