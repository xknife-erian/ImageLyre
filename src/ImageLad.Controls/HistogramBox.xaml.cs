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
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace ImageLad.Controls
{
    /// <summary>
    /// HistogramBox.xaml 的交互逻辑
    /// </summary>
    public partial class HistogramBox : UserControl
    {
        public HistogramBox()
        {
            InitializeComponent();
            Loaded += (s, e) => {_Plot_.InvalidatePlot(); };
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
            DependencyProperty.Register($"{nameof(GrayHistogramSeries)}", typeof(GrayHistogram), typeof(HistogramBox),
                new PropertyMetadata(
                    null,
                    HasData,
                    null));

        private static void HasData(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var plot = ((HistogramBox) d)._Plot_;
            var model = new PlotModel();

            var linearAxis1 = new LinearAxis();
            linearAxis1.Position = AxisPosition.Bottom;
            linearAxis1.IsAxisVisible = false;
            linearAxis1.MaximumPadding = 0;
            linearAxis1.MinimumPadding = 0; 
            model.Axes.Add(linearAxis1);

            var linearAxis2 = new LinearAxis();
            linearAxis2.Position = AxisPosition.Left;
            linearAxis2.IsAxisVisible = false;
            linearAxis2.MaximumPadding = 0;
            linearAxis2.MinimumPadding = 0; 
            model.Axes.Add(linearAxis2);

            var series = new OxyPlot.Series.HistogramSeries();
            model.Series.Add(series);

            var items = (GrayHistogram)e.NewValue;
            for (int i = 0; i < items.Array.Length; i++)
            {
                series.Items.Add(new OxyPlot.Series.HistogramItem((double)i, (double)i+1, items.Array[i], (int)items.Array[i]));
            }

            plot.Model = model;
        }
    }
}

//     /// <summary>
//     /// 实时绘制直方图
//     /// </summary>
//     private void DrawHistogram(DrawingContext dc)
//     {
//         //画背景色，相当于清空整个画布
//         dc.DrawRectangle(Background, null, new Rect(0, 0, ActualWidth, ActualHeight));
//         var max = DataSource.Max();//得到最大的值，即柱状图(直方图实际上就是柱状图)最高的柱。
//         var count = DataSource.Length;
//         if (ActualWidth <= 0 || ActualHeight <= 0)
//             return;
//         var width = ActualWidth;
//         var height = ActualHeight;
//         var grayAxle = 12;//x轴，用黑到白的渐变表示
//         var minGrayAxle = height / 6;//当直方图高度很小时，x轴至少占整图的1/6高度
//         if (minGrayAxle < grayAxle)
//             grayAxle = (int) minGrayAxle;
//         //x轴的渐变笔刷
//         var linearGradientBrush = new LinearGradientBrush(Colors.Black, Colors.White, 0);
//         //画x轴
//         dc.DrawRectangle(linearGradientBrush, null, new Rect(0, height - grayAxle, width, grayAxle));
//         height -= grayAxle;//直方图的高度
//         var bl = height / max; //根据最大值计算出y轴高度的换算比率
//         var w = width / count; //每个值的宽度
//         var b = new SolidColorBrush(Colors.DarkSlateBlue);//画柱子的笔刷
//         for (var i = 0; i < count; i++)
//         {
//             var rh = DataSource[i] * bl;
//             var rect = new Rect(w * i, height - rh, w, rh);
//             dc.DrawRectangle(b, null, rect);//画柱子
//         }
//         //画边界(border)
//         dc.DrawRectangle(null, new Pen(Brushes.Black, 0.1), new Rect(0, 0, ActualWidth - 1, ActualHeight - 1));
//     }
//
//     /// <summary>
//     /// 当有源或源有变化时，重绘画布。
//     /// </summary>
//     private void OnHasSource()
//     {
//         using var dc = _drawingVisual.RenderOpen();
//         DrawHistogram(dc);
//     }
