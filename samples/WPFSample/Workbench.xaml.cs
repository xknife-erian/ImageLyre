using System.Windows;

namespace WPFSample
{
    /// <summary>
    /// Interaction logic for Workbench.xaml
    /// </summary>
    public partial class Workbench : Window
    {
        public Workbench()
        {
            DataContext = new WorkbenchViewModel();
            InitializeComponent();
        }
    }
}
