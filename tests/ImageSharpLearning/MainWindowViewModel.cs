using System;
using System.IO;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace ImageSharpLearning;

public class MainWindowViewModel : ObservableRecipient
{
    private readonly string _file;

    public MainWindowViewModel(string file)
    {
        _file = file;
    }

    public ICommand RGB2GrayCommand => new RelayCommand(() =>
    {
    });
}