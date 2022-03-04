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
        WeakReferenceMessenger.Default.Register<ApplyDialogMessage>(this, (r, m) =>
        {
            var t = m.DialogType;
            switch (t)
            {
                case DialogType.OpenImage:
                    var dialog = new VistaOpenFileDialog();
                    var fs = "*.tif;*.tiff;*.png;*.jpg;*.png;*.jpeg;*.bmp;*.gif";
                    dialog.Filter = $"图像文件|{fs}|All files(*.*)|*.*";
                    var ds = dialog.ShowDialog(this);
                    if (ds.HasValue && ds.Value)
                    {
                        var fn = dialog.FileNames;
                        foreach (var s in fn)
                        {
                            
                        }
                    }
                    break;
            }
        });
    }
}