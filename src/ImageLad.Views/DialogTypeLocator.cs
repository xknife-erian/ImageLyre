using System;
using System.ComponentModel;
using ImageLad.ViewModels;
using ImageLad.Views.Views;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using MvvmDialogs.DialogTypeLocators;

namespace ImageLad.Views;

public class DialogTypeLocator : IDialogTypeLocator
{
    public WorkbenchViewModel MainWindow => Ioc.Default.GetRequiredService<WorkbenchViewModel>();

    public Type Locate(INotifyPropertyChanged viewModel)
    {
        var vmName = viewModel.GetType().Name;
        switch (vmName)
        {
            case nameof(ImageViewModel):
                return typeof(ImageWindow);
            case nameof(LoggerViewModel):
                return typeof(LoggerWindow);
            case nameof(OptionViewModel):
                return typeof(OptionWindow);
            case nameof(HistogramViewModel):
                return typeof(HistogramWindow);
            default:
                throw new NotImplementedException(vmName);
        }
    }
}