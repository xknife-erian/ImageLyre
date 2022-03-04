using ImageLaka.Views.Dialogs;
using Microsoft.Toolkit.Mvvm.Messaging;
using Ookii.Dialogs.Wpf;

namespace ImageLaka.Views;

/// <summary>
///     Interaction logic for Workbench.xaml
/// </summary>
public partial class Workbench : IRecipient<ApplyDialogMessage>
{
    public Workbench()
    {
        InitializeComponent();
        _Ribbon_.SelectedTabIndex = 0;
    }

    public void Receive(ApplyDialogMessage message)
    {
        var t = message.DialogType;
        switch (t)
        {
            case DialogType.OpenImage:
                Ookii.Dialogs.Wpf.VistaOpenFileDialog dialog = new VistaOpenFileDialog() {Filter = "*.txt"};
                dialog.ShowDialog(this);
                break;
        }
    }
}