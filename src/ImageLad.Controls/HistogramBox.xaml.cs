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
using LiveChartsCore;

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
        }

        /// <summary>
        /// 直方图的数据源。
        /// </summary>
        [Category("数据源")]
        public IEnumerable<ISeries> HistogramSeries
        {
            get => (IEnumerable<ISeries>) GetValue(HistogramSeriesProperty);
            set => SetValue(HistogramSeriesProperty, value);
        }

        public static readonly DependencyProperty HistogramSeriesProperty =
            DependencyProperty.Register($"{nameof(HistogramSeries)}", typeof(IEnumerable<ISeries>), typeof(HistogramBox),
                new PropertyMetadata(
                    null,
                    null,
                    null));

        /// <summary>
        /// 直方图的数据源。
        /// </summary>
        [Category("数据源")]
        public int Max
        {
            get => (int)GetValue(MaxProperty);
            set => SetValue(MaxProperty, value);
        }

        public static readonly DependencyProperty MaxProperty =
            DependencyProperty.Register($"{nameof(Max)}", typeof(int), typeof(HistogramBox),
                new PropertyMetadata(
                    255,
                    null,
                    null));
    }
}
