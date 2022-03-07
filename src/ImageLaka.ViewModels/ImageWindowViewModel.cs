using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageLaka.ImageEngine;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using MvvmDialogs;

namespace ImageLaka.ViewModels
{
    public class ImageWindowViewModel : ObservableRecipient
    {
        private ImageReader _imageReader;

        public void Read(string path)
        {
            _imageReader = new ImageReader(path);
            Data = _imageReader.Read();
        }

        public BitmapData Data { get; set; }
    }

}
