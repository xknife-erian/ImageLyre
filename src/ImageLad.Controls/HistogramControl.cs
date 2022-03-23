using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ImageLad.Controls;

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
        //为本控制定义一个唯一的绘图器
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

    #endregion

    #region 依赖属性：数据源

    /// <summary>
    /// 直方图的数据源。
    /// </summary>
    [Category("数据源")]
    public double[] DataSource
    {
        get => (double[]) GetValue(DataSourceProperty);
        set => SetValue(DataSourceProperty, value);
    }

    public static readonly DependencyProperty DataSourceProperty =
        DependencyProperty.Register($"{nameof(DataSource)}", typeof(double[]), typeof(HistogramControl),
            new PropertyMetadata(
                new double[256],
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

    /// <summary>
    /// 当有源或源有变化时，重绘画布。
    /// </summary>
    private void OnHasSource()
    {
        using var dc = _drawingVisual.RenderOpen();
        DrawHistogram(dc);
    }

    #endregion
}