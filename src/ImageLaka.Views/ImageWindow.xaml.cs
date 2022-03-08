using System.Windows;
using ImageLaka.ViewModels;

namespace ImageLaka.Views;

/// <summary>
///     ImageWindow.xaml 的交互逻辑
/// </summary>
public partial class ImageWindow : Window
{
    public ImageWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var vm = (ImageWindowViewModel) DataContext;
        Height = _ToolBar_.Height + vm.Height + _StatusBar_.Height;
        Width = vm.Width;
    }
}