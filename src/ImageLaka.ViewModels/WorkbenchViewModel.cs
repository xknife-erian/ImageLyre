using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace ImageLaka.ViewModels;

public class WorkbenchViewModel : ObservableRecipient
{
    public ICommand NewImage { get; set; }
    public ICommand OpenImage { get; set; }
}