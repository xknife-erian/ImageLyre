using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using System.Windows.Shapes;
using ImageLad.ViewModels;

namespace ImageLad.Views.Views
{
    /// <summary>
    /// ChartWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ChartWindow : Window
    {
        public ChartWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void Datas_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            var ni = e.NewItems;
                var array = new double[ni.Count];
            Utils.DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                foreach (var i in ni)
                {
                    _Plot_.Plot.AddBar(array);
                }

                _Plot_.Refresh();
            });
        }

        // public PlotModel Convert(IEnumerable<double> array)
        // {
        //     var model = new PlotModel();
        //     var lineSerial = new LineSeries() { Title = "Hello" };
        //     for (int i = 0; i < array.Count(); i++)
        //     {
        //         lineSerial.Points.Add(new DataPoint(i, array.ElementAt(i)));
        //     }
        //     model.Series.Add(lineSerial);
        //     return model;
        // }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var vm = (ChartViewModel)DataContext;
            Top = vm.Location.Y;
            Left = vm.Location.X;
            vm.DataSource.CollectionChanged += Datas_CollectionChanged;

        }
    }
}
