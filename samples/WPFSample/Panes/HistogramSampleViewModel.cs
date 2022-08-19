using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using ImageLyre.ImageEngine;
using ImageLyre.ImageEngine.Analyze;
using ImageLyre.UI.Controls;
using ImageLyre.UI.ViewModels;
using ImageLyre.UI.Views.Utils;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using MvvmDialogs;
using OpenCvSharp;

namespace WPFSample.Panes;

public class HistogramSampleViewModel : ObservableRecipient
{
    private static readonly Random _Random = new((int) DateTime.Now.Ticks);
    private readonly IDialogService _dialogService;

    private readonly List<Color> _colors = new();
    private ObservableCollection<UiGrayHistogram> _histograms = new();
    private string _info = string.Empty;
    private ushort _photoCount = 3;
    private ObservableCollection<Mat> _photos = new();
    private int _selectedIndex;

    public HistogramSampleViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set => SetProperty(ref _selectedIndex, value);
    }

    public ObservableCollection<Mat> Photos
    {
        get => _photos;
        set => SetProperty(ref _photos, value);
    }

    /// <summary>
    ///     直方图数据
    /// </summary>
    public ObservableCollection<UiGrayHistogram> Histograms
    {
        get => _histograms;
        set => SetProperty(ref _histograms, value);
    }

    public string Info
    {
        get => _info;
        set => SetProperty(ref _info, value);
    }

    public ushort PhotoCount
    {
        get => _photoCount;
        set => SetProperty(ref _photoCount, value);
    }

    public ICommand ReadPhotosCommand => new RelayCommand(ReadPhotos);

    public ICommand SelectionChangedCommand => new RelayCommand<object>(SelectionChanged);

    private void SelectionChanged(object? obj)
    {
        if (obj == null || obj is not IList list || list.Count<=0)
            return;
        SelectedIndex = _photos.IndexOf((Mat) list[0]);
    }

    private void ReadPhotos()
    {
        string[] photoFiles;
        _colors.Clear();
        Photos.Clear();
        Histograms.Clear();
        var pvm = new ProgressViewModel();
        Task.Factory.StartNew(() =>
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @".\Assets\HistogramSample\");
            if (!Directory.Exists(path))
                return;
            photoFiles = Directory.GetFiles(path);
            var i = 0;
            pvm.Minimum = 0;
            pvm.Maximum = PhotoCount * 2;
            foreach (var file in photoFiles)
            {
                i++;
                if (i > PhotoCount)
                    break;
                var target = new ImageTarget(file);
                target.Open();
                var bmp = target.BmpMat;
                UI.RunAsync(() =>
                {
                    Photos.Add(bmp);
                    pvm.Current = i;
                    pvm.Message = $"{i} - {new FileInfo(file).Name}";
                });
            }

            i = 0;
            var sw = new Stopwatch();
            while (i < PhotoCount)
            {
                sw.Restart();
                Mat mat = new Mat(photoFiles[i], ImreadModes.Grayscale);
                var histogram = GrayHistogram.Compute(mat);
                sw.Stop();
                var e = sw.ElapsedMilliseconds;
                var color = GetColor();
                UI.RunAsync(() =>
                {
                    Histograms.Add(new UiGrayHistogram
                    {
                        Histogram = histogram,
                        Color = color,
                        Visible = true
                    });
                    pvm.Current = i;
                    pvm.Message = $"{i} - {e}";
                    _colors.Add(color);
                });
                i++;
            }

            pvm.Finish();
        });
        _dialogService.ShowDialog(this, pvm);
    }

    private static Color GetColor()
    {
        var color = Color.FromArgb(_Random.Next(255), _Random.Next(255), _Random.Next(255));
        return color;
    }
}