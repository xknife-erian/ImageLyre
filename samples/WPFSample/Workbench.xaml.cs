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
            DataContext = new WorkbenchViewModel();
            InitializeComponent();
        }
    }
}
