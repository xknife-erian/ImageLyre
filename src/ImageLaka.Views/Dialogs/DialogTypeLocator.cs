﻿using System;
using System.ComponentModel;
using ImageLaka.ViewModels;
using MvvmDialogs.DialogTypeLocators;

namespace ImageLaka.Views.Dialogs;

public class DialogTypeLocator: IDialogTypeLocator
{
    public Type Locate(INotifyPropertyChanged viewModel)
    {
        var vmName = viewModel.GetType().Name;
        switch (vmName)
        {
            case nameof(ImageWindowViewModel):
                return typeof(ImageWindow);
            default:
                return null;
        }
    }
}