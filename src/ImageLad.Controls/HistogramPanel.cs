// using System;
// using System.Collections.Generic;
// using System.ComponentModel;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using System.Windows;
// using System.Windows.Controls;
// using System.Windows.Data;
// using System.Windows.Documents;
// using System.Windows.Input;
// using System.Windows.Media;
// using System.Windows.Media.Imaging;
// using System.Windows.Navigation;
// using System.Windows.Shapes;
// using LiveChartsCore;
//
// namespace ImageLad.Controls
// {
//     /// <summary>
//     /// </summary>
//     public class HistogramPanel : Control
//     {
//         static HistogramPanel()
//         {
//             var metaData = new FrameworkPropertyMetadata(typeof(HistogramPanel));
//             DefaultStyleKeyProperty.OverrideMetadata(typeof(HistogramPanel), metaData);
//         }
//
//         /// <summary>
//         /// 直方图的数据源。
//         /// </summary>
//         [Category("数据源")]
//         public IEnumerable<ISeries> DataSource
//         {
//             get => (IEnumerable<ISeries>)GetValue(DataSourceProperty);
//             set => SetValue(DataSourceProperty, value);
//         }
//         
//         public static readonly DependencyProperty DataSourceProperty =
//             DependencyProperty.Register($"{nameof(DataSource)}", typeof(IEnumerable<ISeries>), typeof(HistogramPanel),
//                 new PropertyMetadata(
//                     null,
//                     null,
//                     null));
//
//
//         /*
//         #region 依赖属性：数据源
//
//         /// <summary>
//         /// 直方图的数据源。
//         /// </summary>
//         [Category("数据源")]
//         public double[] HistogramSeries
//         {
//             get => (double[])GetValue(HistogramSeriesProperty);
//             set => SetValue(HistogramSeriesProperty, value);
//         }
//
//         public static readonly DependencyProperty HistogramSeriesProperty =
//             DependencyProperty.Register($"{nameof(HistogramSeries)}", typeof(double[]), typeof(HistogramPanel),
//                 new PropertyMetadata(
//                     new double[256],
//                     OnValueChanged,
//                     OnBinding));
//
//         private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//         {
//             var target = d as HistogramPanel;
//             if (target == null)
//                 throw new NullReferenceException();
//             target.OnHasSource();
//         }
//
//         private static object OnBinding(DependencyObject d, object baseValue)
//         {
//             var target = d as HistogramPanel;
//             if (target == null)
//                 throw new NullReferenceException();
//             target.OnHasSource();
//             return baseValue;
//         }
//
//         /// <summary>
//         /// 当有源或源有变化时，重绘画布。
//         /// </summary>
//         private void OnHasSource()
//         {
//         }
//
//         #endregion
//         */
//     }
// }
