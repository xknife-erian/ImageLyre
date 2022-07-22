using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using ImageLad.ImageEngine;
using ImageLad.ImageEngine.Analyze;
using ImageLad.UI.Controls;
using ImageLad.UI.ViewModels;
using ImageLad.UI.Views.Utils;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using MvvmDialogs;

namespace WPFSample.Panes
{
    public class HistogramSampleViewModel : ObservableRecipient
    {
        private readonly string[] _images;
        private int _currentImage =0;
        private IDialogService _dialogService;

        public HistogramSampleViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @".\Assets\HistogramSample\");
            if (!Directory.Exists(path))
                return;
            _images = Directory.GetFiles(path);
        }

        private void ReadPhotos()
        {
            _dialogService.Show(this, new ProgressViewModel());
            var i = 0;
            foreach (var file in _images)
            {
                if (i > 5)
                    break;
                i++;
                var target = new ImageTarget(file);
                target.Open();
                var bmp = target.Bitmap;
                Photos.Add(bmp);

                var histogram = GrayHistogram.Compute(bmp, GrayFormula.Weighted);
                Histograms.Add(new()
                {
                    Histogram = histogram,
                    Color = Color.CadetBlue,
                    Visible = true
                });

                var fileInfo = new FileInfo(file);
                Info = $"{fileInfo.Name.ToUpper()}, {fileInfo.Length / 1000}k, {histogram}";
            }
        }

        private void ReadImage()
        {
            var file = _images[_currentImage];

            var target = new ImageTarget(file);
            target.Open();
            var bmp = target.Bitmap;

            var histogram = GrayHistogram.Compute(bmp, GrayFormula.Weighted);
            Histograms = new List<UiGrayHistogram> {new()
            {
                Histogram = histogram,
                Color = Color.CadetBlue,
                Visible = true
            }};

            var fileInfo = new FileInfo(file);
            Info = $"{fileInfo.Name.ToUpper()}, {fileInfo.Length / 1000}k, {histogram}";
            Image = bmp;
        }

        #region 为界面准备的可被绑定的属性
        
        private List<UiGrayHistogram> _histograms = new();
        private string _info;
        private Bitmap _image;

        /// <summary>
        /// 直方图数据
        /// </summary>
        public List<UiGrayHistogram> Histograms
        {
            get => _histograms;
            set => SetProperty(ref _histograms, value);
        }

        public string Info
        {
            get => _info;
            set => SetProperty(ref _info, value);
        }

        public Bitmap Image
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }

        #endregion

        public ObservableCollection<Bitmap> Photos { get; set; } = new();

        public ICommand ReadPhotosCommand => new RelayCommand(ReadPhotos);

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
    }
}
