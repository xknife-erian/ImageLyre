using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ImageLad.ImageEngine;
using ImageLad.ImageEngine.Analyze;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace ImageLad.Controls
{
    /// <summary>
    /// HistogramPanel.xaml 的交互逻辑
    /// </summary>
    public partial class HistogramPanel : UserControl
    {
        public HistogramPanel()
        {
            InitializeComponent();
            Loaded += (s, e) => { _Plot_.InvalidatePlot(); };
        }

        /// <summary>
        /// 直方图的数据源。
        /// </summary>
        public GrayHistogram GrayHistogramSeries
        {
            get => (GrayHistogram) GetValue(GrayHistogramSeriesProperty);
            set => SetValue(GrayHistogramSeriesProperty, value);
        }


        public static readonly DependencyProperty GrayHistogramSeriesProperty =
            DependencyProperty.Register($"{nameof(GrayHistogramSeries)}", typeof(GrayHistogram), typeof(HistogramPanel),
                new PropertyMetadata(
                    null,   // 依赖项对象的默认值，通常作为某种特定类型的值提供。
                    HasData,// 对处理程序实现的引用，每当属性的有效值更改时，属性系统都将调用该处理程序实现。
                    null)); // 对处理程序实现的引用，每当属性系统对该属性调用 CoerceValue(DependencyProperty) 时都将调用此处理程序实现。

        private static void HasData(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var plot = ((HistogramPanel) d)._Plot_;
            if (plot.Model == null)
            {
                var model = new PlotModel();

                var bottomAxis = new LinearAxis();
                bottomAxis.Position = AxisPosition.Bottom;
                bottomAxis.IsAxisVisible = false;
                bottomAxis.MaximumPadding = 0;
                bottomAxis.MinimumPadding = 0;
                model.Axes.Add(bottomAxis);

                var leftAxis = new LinearAxis();
                leftAxis.Position = AxisPosition.Left;
                leftAxis.IsAxisVisible = false;
                leftAxis.MaximumPadding = 0;
                leftAxis.MinimumPadding = 0;
                model.Axes.Add(leftAxis);

                var series = new HistogramSeries();
                series.FillColor = OxyColor.FromRgb(110, 130, 240);
                model.Series.Add(series);

                plot.Model = model;
            }

            var hs = (HistogramSeries) plot.Model.Series[0];
            var gh = (GrayHistogram) e.NewValue;
            hs.Items.Clear();
            for (int i = 0; i < gh.Array.Length; i++)
            {
                hs.Items.Add(new HistogramItem(i, (double) i + 1, gh.Array[i], (int) gh.Array[i]));
            }
            plot.InvalidatePlot();
        }
    }
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