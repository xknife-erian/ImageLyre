using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ImageLad.Views.Utils;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = SixLabors.ImageSharp.Color;
using Image = SixLabors.ImageSharp.Image;

namespace ImageSharpLearning
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = new MainWindowViewModel("06.jpg");
            InitializeComponent();
        }
    }

    public class MainWindowViewModel : ObservableRecipient
    {
        private string _file;
        private ImageSharpSource<Rgb24> _image;

        public MainWindowViewModel(string file)
        {
            _file = file;
            Image = new ImageSharpSource<Rgb24>(GetImage());
        }

        private Image<Rgb24> GetImage()
        {
            var stream = new MemoryStream(File.ReadAllBytes(_file));
            var image = SixLabors.ImageSharp.Image.Load(stream);
            return (Image<Rgb24>) image;
        }

        public ImageSharpSource<Rgb24> Image
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }

        public ICommand RGB2GrayCommand => new RelayCommand(() =>
        {
            var c = new ColorSpaceConverter();
            var image = GetImage();
            var newImg = new ImageSharpSource<L8>();
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    Span<Rgb24> row = accessor.GetRowSpan(y);

                    // pixelRow.Length has the same value as accessor.Width,
                    // but using pixelRow.Length allows the JIT to optimize away bounds checks:
                    for (int x = 0; x < row.Length; x++)
                    {
                        // Get a reference to the pixel at position x
                        //pixelRow[x] = new Rgb24((byte) x, (byte) y, (byte) (x - y));
                        var rx = row[x];
                    }
                }

                Image = new ImageSharpSource<Rgb24>(image);
            });
        });
    }
}
