using System;
using System.ComponentModel;
using ImageLad.ViewModels;
using ImageLad.Views.Views;
using MvvmDialogs.DialogTypeLocators;

namespace ImageLad.Views.Dialogs;

public class DialogTypeLocator: IDialogTypeLocator
{
    public Type Locate(INotifyPropertyChanged viewModel)
    {
        var vmName = viewModel.GetType().Name;
        switch (vmName)
        {
            case nameof(ImageWindowViewModel):
                return typeof(ImageWindow);
            case nameof(LoggerWindowViewModel):
                return typeof(LoggerWindow);
            default:
                return null;
        }
    }
}