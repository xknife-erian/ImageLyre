using System;
using System.ComponentModel;
using ImageLad.UI.ViewModels;
using ImageLad.UI.Views.Views;
using MvvmDialogs.DialogTypeLocators;

namespace ImageLad.UI.Views;

public class DialogTypeLocator : IDialogTypeLocator
{
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