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

    public ICommand NewImageFileCommand => new RelayCommand(NewImageFile);

    public ICommand OpenImageFileCommand => new RelayCommand(OpenImageFile);

    public ICommand To8BitCommand => new RelayCommand(To8Bit);

    public ICommand To16BitCommand => new RelayCommand(To16Bit);

    public ICommand SwitchLanguageCommand => new RelayCommand(SwitchLanguage);

    public ICommand ViewAppLogCommand => new RelayCommand(ViewAppLog);

    private void To8Bit()
    {
        if (ImageFiles == null)
            return;
        ImageWindowViewModel vm;
        if (_imageViewModelMap.TryGetValue(ImageFiles, out vm))
        {
            vm.To8Bit();
        }
    }

    private void To16Bit()
    {
        if (ImageFiles == null)
            return;
        ImageWindowViewModel vm;
        if (!_imageViewModelMap.ContainsKey(ImageFiles))
        {
            vm = _imageWindowViewModelFactory.Invoke();
            vm.To16Bit();
        }
    }

    private void NewImageFile()
    {
    }

    private void OpenImageFile()
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
            ImageWindowViewModel vm;
            if (_imageViewModelMap.ContainsKey(ImageFiles))
            {
                vm = _imageViewModelMap[ImageFiles];
            }
            else
            {
                vm = _imageWindowViewModelFactory.Invoke();
                vm.Read(ImageFiles);
                _imageViewModelMap.Add(ImageFiles, vm);
            }

            _dialogService.Show(this, vm);
        }
    }
    private void SwitchLanguage()
    {

    }

    private void ViewAppLog()
    {
    }
}