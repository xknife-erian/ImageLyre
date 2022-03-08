using System.Windows;
using ImageLaka.ViewModels;
using ImageLaka.Views.Dialogs;
using Microsoft.Toolkit.Mvvm.Messaging;
using Ookii.Dialogs.Wpf;

namespace ImageLaka.Views;

/// <summary>
///     Interaction logic for Workbench.xaml
/// </summary>
public partial class Workbench
{
    public Workbench()
    {
        InitializeComponent();
        Loaded+= delegate(object sender, RoutedEventArgs args)
        {
            var vm = (WorkbenchViewModel) DataContext;
            
        };
        _Ribbon_.SelectedTabIndex = 0;

    }

}