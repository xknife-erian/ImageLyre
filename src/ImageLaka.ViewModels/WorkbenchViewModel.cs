using System.Windows.Input;
using ImageLaka.Views;
using ImageLaka.Views.Dialogs;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace ImageLaka.ViewModels;

public class WorkbenchViewModel : ObservableRecipient
{
    public ICommand NewImage { get; set; }

    public ICommand OpenImage
    {
        get
        {
            return new AsyncRelayCommand(() =>
            {
                return Task.Factory.StartNew(() =>
                {
                    WeakReferenceMessenger.Default.Send(new ApplyDialogMessage(DialogType.OpenImage));
                });
            });
        }
    }
}