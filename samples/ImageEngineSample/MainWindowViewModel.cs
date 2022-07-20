using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ImageLad.ImageEngine;
using ImageLad.ImageEngine.Jobs;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Color = System.Drawing.Color;
using Size = System.Windows.Size;

namespace ImageEngineSample;

public class MainWindowViewModel : ObservableRecipient
{
    private WriteableBitmap _bitmap;

    private byte[] _byteSpan;

    private readonly ImageBuilder _imageBuilder = new();
    private readonly Int32Rect _imageRect = new(0, 0, 512, 512);
    private readonly ImageProcessor _processor = new();
    private readonly AutoResetEvent _resetEvent = new(false);
    private readonly ImageProcessSetting setting;

    public MainWindowViewModel()
    {
        _photoBytes = GetPhotos();
        setting = new ImageProcessSetting
        {
            ImageRect = new Size(_imageRect.Width, _imageRect.Height)
        };
        Bitmap = new WriteableBitmap(_imageRect.Width, _imageRect.Height, 96, 96, PixelFormats.Bgr24, null);
        _processor.Initialize("test", SrcBufferGetter, BufferSetter, setting);
    }

    public WriteableBitmap Bitmap
    {
        get => _bitmap;
        set => SetProperty(ref _bitmap, value);
    }

    private void BufferSetter(byte[] obj)
    {
        _byteSpan = obj;
        _resetEvent.Set();
    }
    
    private byte[] SrcBufferGetter()
    {
        return _imageBuilder.Buffer;
    }

    public void Start()
    {
        _imageBuilder.Start();
        _processor.Start();
    }

    public void DisplayBitmapBytes()
    {
        Task.Factory.StartNew(() =>
        {
            while (true)
            {
                _resetEvent.WaitOne();
                if (_byteSpan.Length == 0)
                    continue;
                Application.Current.Dispatcher.Invoke(
                    DispatcherPriority.Normal, (Action) delegate
                    {
                        _bitmap.Lock();
                        _bitmap.WritePixels(_imageRect, _byteSpan, 512 * 4, 0);
                        _bitmap.AddDirtyRect(_imageRect);
                        _bitmap.Unlock();
                    });
            }
        });
    }

    #region DisplayPhoto

    private bool _displayPhotoEnable = true;
    private float _fps = 1.2F;
    private readonly List<byte[]> _photoBytes;
    private List<WriteableBitmap> _photos;
    private WriteableBitmap _photo;

    public WriteableBitmap Photo
    {
        get => _photo;
        set => SetProperty(ref _photo, value);
    }

    public float Fps
    {
        get => _fps;
        set => SetProperty(ref _fps, value);
    }

    public ICommand DrawBytesCommand => new RelayCommand(() =>
    {
        var bc = 32;
        var lo = new List<Color>(3)
        {
            Color.DarkBlue,
            Color.YellowGreen,
            Color.White,
            Color.Red
        };
        var size = 512 * 512 * 4;
        var bytes = new byte[size + 512];
        var k = 0;
        for (int i = 0; i < size; i += 4)
        {
            Color color = lo[k];
            if (i != 0 && i % (bc * 4) == 0)
            {
                k++;
                if (k == lo.Count)
                    k = 0;
            }

            bytes[i] = color.B;
            bytes[i + 1] = color.G;
            bytes[i + 2] = color.R;
            bytes[i + 3] = 0x00;
        }

        Photo.Lock();
        Photo.WritePixels(new Int32Rect(0, 0, 512, 512), bytes, Photo.BackBufferStride, 4);
        Photo.AddDirtyRect(_imageRect);
        Photo.Unlock();
    });

    private List<byte[]> GetPhotos()
    {
        _photos = new List<WriteableBitmap>();
        var list = new List<byte[]>();
        var path = @".\images\";
        var files = Directory.GetFiles(path);
        foreach (var file in files)
        {
            //将照片转为字节数组
            var bitmap = new Bitmap(file);
            using var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Bmp);
            var bytes = ms.GetBuffer();
            list.Add(bytes);

            //为对比显示，得到直接的照片
            using var fs = new FileStream(file, FileMode.Open, FileAccess.Read);
            _photos.Add(WriteableBitmapUtil.BuildBitmap(fs));
        }

        Photo = _photos[1];
        return list;
    }

    /// <summary>
    ///     循环显示5幅图片
    /// </summary>
    public void StartDisplayPhoto()
    {
        _displayPhotoEnable = true;
        Task.Factory.StartNew(() =>
        {
            for (var i = 0; i < _photoBytes.Count; i++)
            {
                if (!_displayPhotoEnable)
                    break;
                var index = i;
                Application.Current.Dispatcher.Invoke(
                    DispatcherPriority.Normal, (Action) delegate
                    {
                        Photo = _photos[index];
                        _bitmap.Lock();
                        var stride = (_imageRect.Width * _bitmap.Format.BitsPerPixel) / 8;
                        var array = new byte[_photoBytes[index].Length];
                        Array.Copy(_photoBytes[index], array, _photoBytes[index].Length);
                        BytesUtil.ReverseImageStream(_photoBytes[index], array, 3);
                        _bitmap.WritePixels(_imageRect, array, _bitmap.BackBufferStride, 0);
                        _bitmap.AddDirtyRect(_imageRect);
                        _bitmap.Unlock();
                    });
                if (i == _photoBytes.Count - 1)
                    i = -1;
                Thread.Sleep((int) (1000 / Fps));
            }
        });
    }

    public void StopDisplayPhoto()
    {
        _displayPhotoEnable = false;
        Thread.Sleep(50);
    }

    #endregion
}

public class ImageBuilder
{
    private const string PATH = @"Z:\frames\";

    private readonly Thread _mainThread;

    public ImageBuilder()
    {
        _mainThread = new Thread(Read) {IsBackground = true};
    }

    public byte[] Buffer { get; set; }

    public void Start()
    {
        _mainThread.Start();
    }

    private void Read()
    {
        if (!Directory.Exists(PATH))
            return;
        var files = Directory.GetFiles(PATH);
        foreach (var file in files)
        {
            var fs = new FileStream(file, FileMode.Open, FileAccess.Read);
            Buffer = new byte[fs.Length];
            fs.Read(Buffer, 0, (int) fs.Length);
            Thread.Sleep(100);
        }
    }
}