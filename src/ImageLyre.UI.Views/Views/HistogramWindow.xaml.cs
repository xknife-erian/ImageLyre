using System.Windows;
using ImageLyric.UI.ViewModels;

namespace ImageLyric.UI.Views.Views
{
    /// <summary>
    /// HistogramWindow.xaml 的交互逻辑
    /// </summary>
    public partial class HistogramWindow : Window
    {
        public HistogramWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var vm = (HistogramViewModel)DataContext;
            Top = vm.Location.Y;
            Left = vm.Location.X;
        }
    }
}
