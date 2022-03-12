using System.Drawing;
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
        AdjustWindowSize(vm.Bitmap);
    }

    /// <summary>
    /// 调整窗体大小
    /// </summary>
    /// <param name="bmp"></param>
    private void AdjustWindowSize(Bitmap? bmp)
    {
        var bmpHeight = 0;
        if (bmp != null)
            bmpHeight = bmp.Height;
        Height = bmpHeight + _StatusBar_.Height;
        if (bmp != null)
            Width = bmp.Width;
    }
}