using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ImageLaka.Controls;

/// <summary>
///     灰度直方图控件
/// </summary>
public sealed class HistogramControl : Canvas
{
    private readonly DrawingVisual _drawingVisual;

    static HistogramControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(HistogramControl),
            new FrameworkPropertyMetadata(typeof(HistogramControl)));
    }

    public HistogramControl()
    {
        _drawingVisual = new DrawingVisual();
        AddVisualChild(_drawingVisual);
    }

    protected override int VisualChildrenCount => 1;

    protected override Visual GetVisualChild(int index)
    {
        return _drawingVisual;
    }

    #region Overrides of Panel

    /// <summary>
    ///     Draws the content of a <see cref="T:System.Windows.Media.DrawingContext" /> object during the render pass of a
    ///     <see cref="T:System.Windows.Controls.Panel" /> element.
    /// </summary>
    /// <param name="dc">The <see cref="T:System.Windows.Media.DrawingContext" /> object to draw.</param>
    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        DrawHistogram(dc);
    }

    #region Overrides of FrameworkElement

    /// <summary>
    ///     Raises the <see cref="E:System.Windows.FrameworkElement.SizeChanged" /> event, using the specified information
    ///     as part of the eventual event data.
    /// </summary>
    /// <param name="sizeInfo">Details of the old and new size involved in the change.</param>
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        using var dc = _drawingVisual.RenderOpen();
        DrawHistogram(dc);
    }

    #endregion

    private void DrawHistogram(DrawingContext dc)
    {
        dc.DrawRectangle(Background, null, new Rect(0, 0, ActualWidth, ActualHeight));
        var max = DataSource.Max();
        var count = DataSource.Length;
        if (ActualWidth <= 0 || ActualHeight <= 0)
            return;
        var width = ActualWidth;
        var height = ActualHeight;
        var th = 12;
        var ath = height / 6;
        if (ath < th)
            th = (int) ath;
        var lb = new LinearGradientBrush(Colors.Black, Colors.White, 0);
        dc.DrawRectangle(lb, null, new Rect(0, height - th, width, th));
        height -= th;
        var bl = height / max; //高度比率
        var w = width / count;
        var b = new SolidColorBrush(Colors.DarkSlateBlue);
        for (var i = 0; i < count; i++)
        {
            var rh = DataSource[i] * bl;
            var rect = new Rect(w * i, height - rh, w, rh);
            dc.DrawRectangle(b, null, rect);
        }

        dc.DrawRectangle(null, new Pen(Brushes.Black, 0.1), new Rect(0, 0, ActualWidth - 1, ActualHeight - 1));
    }

    #endregion

    #region 依赖属性：数据源

    [Category("数据源")]
    public int[] DataSource
    {
        get => (int[]) GetValue(DataSourceProperty);
        set => SetValue(DataSourceProperty, value);
    }

    public static readonly DependencyProperty DataSourceProperty =
        DependencyProperty.Register($"{nameof(DataSource)}", typeof(int[]), typeof(HistogramControl),
            new PropertyMetadata(
                new int[256],
                OnValueChanged,
                OnBinding));

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var target = d as HistogramControl;
        if (target == null)
            throw new NullReferenceException();
        target.OnHasSource();
    }

    private static object OnBinding(DependencyObject d, object baseValue)
    {
        var target = d as HistogramControl;
        if (target == null)
            throw new NullReferenceException();
        target.OnHasSource();
        return baseValue;
    }

    private void OnHasSource()
    {
        using var dc = _drawingVisual.RenderOpen();
        DrawHistogram(dc);
    }

    #endregion
}