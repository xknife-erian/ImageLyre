using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageLyric.ImageEngine;
using ImageLyric.ImageEngine.LookUpTable;
using Color = System.Drawing.Color;

namespace LUTSample;

/// <summary>
///     MainWindow.xaml 的交互逻辑
/// </summary>
public partial class MainWindow : Window
{
    private const string GRAY_IMAGE_PATH1 = @".\imgs\a01.jpg";
    private const string GRAY_IMAGE_PATH2 = @".\imgs\a02.jpg";

    private readonly Dictionary<string, string> _fileMap = new();

    public MainWindow()
    {
        InitializeComponent();
        BuildLUTFileButton();
        LoadImage();
        Loaded += delegate
        {
            _LUTPanel_.Background = new SolidColorBrush(Colors.Transparent);
            var bitmap = new WriteableBitmap((int) _LUTPanel_.Width, (int) _LUTPanel_.Height, 96, 96,
                PixelFormats.Bgr24, null);
            _LUTPanelImage_.Source = bitmap;
        };
    }

    private void BuildLUTFileButton()
    {
        var path = @".\luts\";
        var files = Directory.GetFiles(path);
        for (var i = 0; i < files.Length; i++)
        {
            var file = Path.GetFileName(files[i]);
            var name = file.Substring(0, file.IndexOf('.'));
            _fileMap.Add(name, files[i]);
            var btn = new Button {Content = name};
            _LutFilesPanel_.Children.Add(btn);
        }
    }

    private void LoadImage()
    {
        var fs = new FileStream(GRAY_IMAGE_PATH1, FileMode.Open);
        var bmpSrc = WriteableBitmapUtil.BuildBitmap(fs);
        _Image1_.Source = bmpSrc;
        fs.Close();

        LoadImage2();
    }

    private WriteableBitmap LoadImage2()
    {
        var fs1 = new FileStream(GRAY_IMAGE_PATH2, FileMode.Open);
        var bmpSrc1 = WriteableBitmapUtil.BuildBitmap(fs1);
        _Image2_.Source = bmpSrc1;
        fs1.Close();
        return bmpSrc1;
    }

    private void PresetLUTButton_Click(object sender, RoutedEventArgs e)
    {
        var button = (Button) sender;
        var loader = new LUTLoader();
        var lut = loader.GetPresetLUT(button.Content.ToString());
        UpdateLUTPanel(lut);
        UpdateImageSourceByLUT(lut);
    }

    private void LUTFileButton_Click(object sender, RoutedEventArgs e)
    {
        var button = (Button) sender;
        var loader = new LUTLoader();
        var lut = loader.OpenLUTFile(_fileMap[button.Content.ToString()]);
        UpdateLUTPanel(lut);
        UpdateImageSourceByLUT(lut);
    }

    private void UpdateLUTPanel(LUT lut)
    {
        _LUTName_.Text = lut.Name;
        var colors = lut.GetColorMap();
        var wBitmap = (WriteableBitmap)_LUTPanelImage_.Source;
        var width = (int)_LUTPanelImage_.Width;
        var height =(int)_LUTPanelImage_.Height;
        wBitmap.Lock();
        Bitmap backBitmap = new Bitmap(width, height, wBitmap.BackBufferStride, System.Drawing.Imaging.PixelFormat.Format24bppRgb, wBitmap.BackBuffer);
        Graphics graphics = Graphics.FromImage(backBitmap);
        graphics.Clear(Color.Transparent);

        for (int i = 0; i < width; i++)
        {
            var j = i / 2;
            int x1 = i;
            int y1 = 0;
            int x2 = i;
            int y2 = height;
            var co = colors[j];
            graphics.DrawLine(new System.Drawing.Pen(Color.FromArgb(255, co.R, co.G, co.B)), x1, y1, x2, y2);
        }

        graphics.Flush();
        graphics.Dispose();

        backBitmap.Dispose();
        wBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
        wBitmap.Unlock();
    }

    private unsafe void UpdateImageSourceByLUT(LUT lut)
    {
        var src = LoadImage2();

        var map = lut.GetColorMap();
        var bytes = (byte*) src.BackBuffer.ToPointer();
        src.Lock();
        for (var i = 0; i < src.BackBufferStride * src.PixelHeight; i += 4)
        {
            var index = bytes[i];
            //当是PixelFormats.Bgr24时的伪彩填充，其他格式时应该改变
            bytes[i] = map[index].B;
            bytes[i + 1] = map[index].G;
            bytes[i + 2] = map[index].R;
        }

        src.AddDirtyRect(new Int32Rect(0, 0, src.PixelWidth, src.PixelHeight));
        src.Unlock();
    }
}