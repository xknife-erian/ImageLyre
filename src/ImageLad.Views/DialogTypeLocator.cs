using System;
using System.ComponentModel;
using ImageLad.ViewModels;
using ImageLad.Views.Views;
using MvvmDialogs.DialogTypeLocators;

namespace ImageLad.Views;

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
            default:
                throw new NotImplementedException(vmName);
        }
    }
}