using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ImageLaka.ImageEngine;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace WPFTest
{
    public class MainViewModel : ObservableRecipient
    {
        private readonly string[] _images;
        private int _currentImage =0;

        public MainViewModel()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"assets\");
            if (!Directory.Exists(path))
                return;
            _images = Directory.GetFiles(path);

            ReadImage();
        }

        private void ReadImage()
        {
            var file = _images[_currentImage];
            var bmp = new Bitmap(file);
            HistogramDataArray = ImageUtil.GetHistogram(bmp);
            var fi = new FileInfo(file);
            CurrentFile = $"{fi.Name.ToUpper()}, {fi.Length / 1000}k";
            Image = CreateBitmapSourceFromBitmap(bmp);
        }

        #region 为界面准备的可被绑定的属性
        
        private int[] _histogramDataArray = new int[256];
        private string _currentFile;
        private BitmapSource _image;

        /// <summary>
        /// 直方图数据
        /// </summary>
        public int[] HistogramDataArray
        {
            get => _histogramDataArray;
            set => SetProperty(ref _histogramDataArray, value);
        }

        public string CurrentFile
        {
            get => _currentFile;
            set => SetProperty(ref _currentFile, value);
        }

        public BitmapSource Image
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }

        #endregion

        public ICommand OpenLastImageCommand => new RelayCommand(LastImage);

        public ICommand OpenNextImageCommand => new RelayCommand(NextImage);

        private void LastImage()
        {
            if (_currentImage == 0)
                _currentImage = _images.Length - 1;
            else
                _currentImage--;
            ReadImage();
        }   
        private void NextImage()
        {
            if (_currentImage == _images.Length - 1)
                _currentImage = 0;
            else
                _currentImage++;
            ReadImage();
        }

        #region  System.Drawing.Bitmap To BitmapSource
        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        public static BitmapSource CreateBitmapSourceFromBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("bitmap");

            lock (bitmap)
            {
                IntPtr hBitmap = bitmap.GetHbitmap();

                try
                {
                    return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        hBitmap,
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
                finally
                {
                    DeleteObject(hBitmap);
                }
            }
        }
        #endregion
    }
}
