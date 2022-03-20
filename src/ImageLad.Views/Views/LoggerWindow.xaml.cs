using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ImageLad.ViewModels;
using ImageLad.Views.Utils;
using NLog;

namespace ImageLad.Views.Views;

/// <summary>
///     LoggerWindow.xaml 的交互逻辑
/// </summary>
public partial class LoggerWindow : Window
{
    public LoggerWindow()
    {
        InitializeComponent();
        Loaded += OnViewLoaded;
        SizeChanged += OnViewSizeChanged;
        Closing += LoggerWindow_Closing;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var vm = (LoggerWindowViewModel)DataContext;
        Top = vm.Location.Y;
        Left = vm.Location.X;
    }

    private void LoggerWindow_Closing(object? sender, CancelEventArgs e)
    {
        Hide();
        e.Cancel = true; // this cancels the close event. 
    }

    private void OnViewLoaded(object sender, RoutedEventArgs e)
    {
        AdjustColumnWidth(RenderSize);
    }

    private void OnViewSizeChanged(object sender, SizeChangedEventArgs e)
    {
        AdjustColumnWidth(e.NewSize);
    }

    private void AdjustColumnWidth(Size size)
    {
        var w = size.Width;
        _TimeColumn_.Width = 98;
        _LevelColumn_.Width = 48;
        _NameColumn_.Width = 96;
        var newWidth = w - _TimeColumn_.Width - _LevelColumn_.Width - _ExColumn_.Width - _NameColumn_.Width - 45;
        if (newWidth > 0)
            _MsgColumn_.Width = newWidth;
    }

    public virtual void ScrollToFirst()
    {
        if (_LogView_.Items.Count <= 0) return;
        _LogView_.SelectedIndex = 0;
        ScrollToItem(_LogView_.SelectedItem);
    }

    public virtual void ScrollToLast()
    {
        if (_LogView_.Items.Count <= 0) return;
        _LogView_.SelectedIndex = _LogView_.Items.Count - 1;
        ScrollToItem(_LogView_.SelectedItem);
    }

    private void ScrollToItem(object item)
    {
        _LogView_.ScrollIntoView(item);
    }
}