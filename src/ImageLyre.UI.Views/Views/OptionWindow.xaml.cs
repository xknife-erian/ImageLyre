using System.Windows;
using ImageLyre.UI.ViewModels;

namespace ImageLyre.UI.Views.Views
{
    /// <summary>
    /// OptionWindow.xaml 的交互逻辑
    /// </summary>
    public partial class OptionWindow : Window
    {
        public OptionWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var vm = (OptionViewModel)DataContext;
            Top = vm.Location.Y;
            Left = vm.Location.X;
        }
    }
}
