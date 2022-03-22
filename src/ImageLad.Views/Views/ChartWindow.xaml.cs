﻿using System;
using System.Collections.Generic;
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

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var vm = (ChartViewModel)DataContext;
            Top = vm.Location.Y;
            Left = vm.Location.X;
        }
    }
}
