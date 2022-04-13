using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
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
            var image = Image.Load(File.Open("06.jpg", FileMode.Open));
            var vm = new MainWindowViewModel();
            vm.Image = new ImageSharpSource<Rgb24>((Image<Rgb24>) image);

            DataContext = vm;

            InitializeComponent();
        }
    }

    public class MainWindowViewModel : ObservableRecipient
    {
        private ImageSharpSource<Rgb24> _image;

        public ImageSharpSource<Rgb24> Image
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }
    }
}
