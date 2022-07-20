using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageEngineSample;

/// <summary>
/// MainWindow.xaml 的交互逻辑
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = (MainWindowViewModel) DataContext;
    }

    private void StartButton_OnClick(object sender, RoutedEventArgs e)
    {
        _viewModel.Start();
    }

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel.DisplayBitmapBytes();
    }

    private void StartPhotoButton_OnClick(object sender, RoutedEventArgs e)
    {
        _viewModel.StartDisplayPhoto();
    }

    private void StopPhotoButton_OnClick(object sender, RoutedEventArgs e)
    {
        _viewModel.StopDisplayPhoto();
    }
}

public partial class WriteableBitmapandthreads : Window
{
    WriteableBitmap wb = new WriteableBitmap(3, 3, 96, 96, PixelFormats.Bgra32, null);
    public WriteableBitmapandthreads()
    {
        for (int i = 0; i < (int)wb.Height; i++)
        {
            byte blue = (byte)(i == 2 ? 255 : 0),
                green = (byte)(i == 1 ? 255 : 0),
                red = (byte)(i == 0 ? 255 : 0);

            byte[] colorData = { blue, green, red, 255 };

            for (int j = 0; j < (int)wb.Width; j++)
            {
                Int32Rect rect = new Int32Rect(j, i, 1, 1);
                wb.WritePixels(rect, colorData, 4, 0);
            }
        }

    }

    private void button_Click(object sender, RoutedEventArgs e)
    {

        // Thread thread = new Thread(new ThreadStart(() =>
        // {
        //     Application.Current.Dispatcher.Invoke(
        //         DispatcherPriority.Normal, (Action)delegate
        //         {
        //             image.Source = wb;
        //
        //             DispatcherTimer _timer = new DispatcherTimer();
        //             TimeSpan _time = new TimeSpan();
        //             _time = TimeSpan.FromSeconds(30);
        //             _timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
        //             {
        //                 image.Source = wb;
        //                 button.Content = "Load " + _time.ToString("c") + " times!";
        //                 if (_time == TimeSpan.Zero) _timer.Stop();
        //                 _time = _time.Add(TimeSpan.FromSeconds(-1));
        //             }, Application.Current.Dispatcher);
        //             _timer.Start();
        //         });
        // }));
        // thread.Start();
    }
}