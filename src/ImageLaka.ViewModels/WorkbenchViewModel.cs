using System.Reflection;
using System.Windows.Input;
using ImageLaka.Views;
using ImageLaka.Views.Dialogs;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;

namespace ImageLaka.ViewModels;

public class WorkbenchViewModel : ObservableRecipient
{
    private readonly IDialogService _dialogService;
    private string? _imageFiles;

    public WorkbenchViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public string? ImageFiles
    {
        get => _imageFiles;
        set => SetProperty(ref _imageFiles, value);
    }

    public ICommand NewImageFileCommand { get; set; }

    public ICommand OpenImageFileCommand => new RelayCommand(OpenFile);

    private void OpenFile()
    {
        var fs = "*.tif;*.tiff;*.png;*.jpg;*.jpeg;*.bmp;*.gif";
        var filter = $"图像文件|{fs}|All files(*.*)|*.*";

        var settings = new OpenFileDialogSettings
        {
            Title = "图像文件",
            Filter = filter
        };

        bool? success = _dialogService.ShowOpenFileDialog(this, settings);
        if (success == true)
        {
            ImageFiles = settings.FileName;
            _dialogService.Show(this, new ImageWindowViewModel());
        }
    }
}