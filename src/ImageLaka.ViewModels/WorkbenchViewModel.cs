using System.Windows.Input;
using ImageLaka.ImageEngine;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;

namespace ImageLaka.ViewModels;

public class WorkbenchViewModel : ObservableRecipient
{
    private readonly IDialogService _dialogService;
    private string? _imageFiles;
    private readonly Func<ImageWindowViewModel> _imageWindowViewModelFactory;
    private readonly Dictionary<string, ImageWindowViewModel> _imageViewModelMap 
        = new Dictionary<string, ImageWindowViewModel>();

    public WorkbenchViewModel(IDialogService dialogService, Func<ImageWindowViewModel> imageWindowViewModelFactory)
    {
        _dialogService = dialogService;
        _imageWindowViewModelFactory = imageWindowViewModelFactory;
    }

    public string? ImageFiles
    {
        get => _imageFiles;
        set => SetProperty(ref _imageFiles, value);
    }

    public ICommand NewImageFileCommand => new RelayCommand(OpenFile);

    public ICommand OpenImageFileCommand => new RelayCommand(OpenFile);

    public ICommand SwitchLanguageCommand => new RelayCommand(OpenFile);

    private void OpenFile()
    {
        var fs = "*.tif;*.tiff;*.png;*.jpg;*.jpeg;*.bmp;*.gif";
        var filter = $"图像文件|{fs}|All files(*.*)|*.*";

        var settings = new OpenFileDialogSettings
        {
            Title = "图像文件",
            Filter = filter
        };

        var success = _dialogService.ShowOpenFileDialog(this, settings);
        if (success == true)
        {
            ImageFiles = settings.FileName;
            var vm = _imageWindowViewModelFactory.Invoke();
            vm.Read(settings.FileName);
            _imageViewModelMap.Add(settings.FileName, vm);
            _dialogService.Show(this, vm);
        }
    }
}