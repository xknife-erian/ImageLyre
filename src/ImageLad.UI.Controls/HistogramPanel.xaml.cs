using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using Accessibility;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace ImageLad.UI.Controls;

/// <summary>
///     HistogramPanel.xaml 的交互逻辑
/// </summary>
public partial class HistogramPanel : UserControl
{
    public HistogramPanel()
    {
        InitializeComponent();
        Loaded += (_, _) => { _Plot_.InvalidatePlot(); };
    }

    #region GrayHistograms

    public static readonly DependencyProperty GrayHistogramsProperty = DependencyProperty.Register(
        nameof(GrayHistograms), typeof(ObservableCollection<UiGrayHistogram>), typeof(HistogramPanel),
        new PropertyMetadata(default(ObservableCollection<UiGrayHistogram>), OnGrayHistogramsChanged));

    public ObservableCollection<UiGrayHistogram> GrayHistograms
    {
        get => (ObservableCollection<UiGrayHistogram>)GetValue(GrayHistogramsProperty);
        set => SetValue(GrayHistogramsProperty, value);
    }

    private static void OnGrayHistogramsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not HistogramPanel panel)
            return;

        var plot = panel._Plot_;
        var histograms = (ObservableCollection<UiGrayHistogram>)e.NewValue;
        histograms.CollectionChanged += (_, he) =>
        {
            switch (he.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    plot.Model.Series.Clear();
                    break;
                case NotifyCollectionChangedAction.Add:
                    foreach (UiGrayHistogram hist in he.NewItems)
                    {
                        var series = BuildHistogramSeries(hist);
                        plot.Model.Series.Add(series);
                        plot.InvalidatePlot();
                    }
                    break;
            }
        };
        plot.Model = new PlotModel();

        var bottomAxis = new LinearAxis
        {
            Position = AxisPosition.Bottom,
            IsAxisVisible = false,
            MaximumPadding = 0,
            MinimumPadding = 0
        };
        plot.Model.Axes.Add(bottomAxis);

        var leftAxis = new LinearAxis
        {
            Position = AxisPosition.Left,
            IsAxisVisible = false,
            MaximumPadding = 0,
            MinimumPadding = 0
        };
        plot.Model.Axes.Add(leftAxis);

        foreach (var hist in histograms)
        {
            hist.VisibleChanged += (_, _) =>
            {
                var index = histograms.IndexOf(hist);
                plot.Model.Series[index].IsVisible = hist.Visible;
                plot.InvalidatePlot();
            };
            var series = BuildHistogramSeries(hist);
            plot.Model.Series.Add(series);
        }

        plot.InvalidatePlot();
    }

    private static HistogramSeries BuildHistogramSeries(UiGrayHistogram hist)
    {
        var color = OxyColor.FromRgb(hist.Color.R, hist.Color.G, hist.Color.B);
        var series = new HistogramSeries { FillColor = color, IsVisible = hist.Visible };
        for (var i = 0; i < hist.Histogram.Array.Length; i++)
            series.Items.Add(new HistogramItem(i, (double)i + 1, hist.Histogram.Array[i],
                (int)hist.Histogram.Array[i]));
        return series;
    }

    #endregion

    #region TopmostIndex

    public static readonly DependencyProperty TopmostIndexProperty = DependencyProperty.Register(
        nameof(TopmostIndex), typeof(int), typeof(HistogramPanel), 
        new PropertyMetadata(0, OnTopmostIndexChanged));

    private static void OnTopmostIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not HistogramPanel panel)
            return;
        var index = (int) e.NewValue;
        var series = panel._Plot_.Model.Series;
        var serie = series[index];
        series.RemoveAt(index);
        series.Add(serie);
        panel._Plot_.InvalidatePlot();
    }

    public int TopmostIndex
    {
        get => (int) GetValue(TopmostIndexProperty);
        set => SetValue(TopmostIndexProperty, value);
    }

    #endregion

    #region GrayScaleSliceEnable

    public static readonly DependencyProperty GrayScaleSliceEnableProperty = DependencyProperty.Register(
        nameof(GrayScaleSliceEnable), typeof(bool), typeof(HistogramPanel),
        new PropertyMetadata(false, OnGrayScaleSliceEnableChanged));

    private static void OnGrayScaleSliceEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not HistogramPanel panel)
            return;
        var p = panel._GrayScaleSliceEnablePanel_;
        if ((bool)e.NewValue)
            p.Visibility = Visibility.Visible;
        else
            p.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    ///     中部灰度指示条是否显示。默认显示。
    /// </summary>
    public bool GrayScaleSliceEnable
    {
        get => (bool)GetValue(GrayScaleSliceEnableProperty);
        set => SetValue(GrayScaleSliceEnableProperty, value);
    }

    #endregion

    #region SliceHeight

    public static readonly DependencyProperty SliceHeightProperty = DependencyProperty.Register(
        nameof(SliceHeight), typeof(int), typeof(HistogramPanel),
        new PropertyMetadata(8, OnSliceHeightChanged));

    private static void OnSliceHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not HistogramPanel panel)
            return;
        panel._GrayScaleSliceEnablePanel_.Height = (int)e.NewValue;
    }

    public int SliceHeight
    {
        get => (int)GetValue(SliceHeightProperty);
        set => SetValue(SliceHeightProperty, value);
    }

    #endregion
}

/****
 * 曾尝试用自定义控件的模式在Canvas上自行绘制，虽然成功，但功能较少，未使用。
/// <summary>
/// 实时绘制直方图
/// </summary>
private void DrawHistogram(DrawingContext dc)
{
    //画背景色，相当于清空整个画布
    dc.DrawRectangle(Background, null, new Rect(0, 0, ActualWidth, ActualHeight));
    var max = DataSource.Max();//得到最大的值，即柱状图(直方图实际上就是柱状图)最高的柱。
    var count = DataSource.Length;
    if (ActualWidth <= 0 || ActualHeight <= 0)
        return;
    var width = ActualWidth;
    var height = ActualHeight;
    var grayAxle = 12;//x轴，用黑到白的渐变表示
    var minGrayAxle = height / 6;//当直方图高度很小时，x轴至少占整图的1/6高度
    if (minGrayAxle < grayAxle)
        grayAxle = (int) minGrayAxle;
    //x轴的渐变笔刷
    var linearGradientBrush = new LinearGradientBrush(Colors.Black, Colors.White, 0);
    //画x轴
    dc.DrawRectangle(linearGradientBrush, null, new Rect(0, height - grayAxle, width, grayAxle));
    height -= grayAxle;//直方图的高度
    var bl = height / max; //根据最大值计算出y轴高度的换算比率
    var w = width / count; //每个值的宽度
    var b = new SolidColorBrush(Colors.DarkSlateBlue);//画柱子的笔刷
    for (var i = 0; i < count; i++)
    {
        var rh = DataSource[i] * bl;
        var rect = new Rect(w * i, height - rh, w, rh);
        dc.DrawRectangle(b, null, rect);//画柱子
    }
    //画边界(border)
    dc.DrawRectangle(null, new Pen(Brushes.Black, 0.1), new Rect(0, 0, ActualWidth - 1, ActualHeight - 1));
}

/// <summary>
/// 当有源或源有变化时，重绘画布。
/// </summary>
private void OnHasSource()
{
    using var dc = _drawingVisual.RenderOpen();
    DrawHistogram(dc);
}
**/