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
        _Ribbon_.SelectedTabIndex = 0;
    }
}