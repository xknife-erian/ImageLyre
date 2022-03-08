using System.ComponentModel;
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
            vm.PropertyChanged += ViewModelPropertyChanged;
        };
        _Ribbon_.SelectedTabIndex = 0;

    }

    private void ViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender == null)
            return;
        var vm = (WorkbenchViewModel) sender;
        switch (e.PropertyName)
        {
            case nameof(vm.ImageFiles):
                if (!string.IsNullOrEmpty(vm.ImageFiles))
                    _Ribbon_.SelectedTabIndex = 1;
                break;
            default:
                break;
        }

    }
}