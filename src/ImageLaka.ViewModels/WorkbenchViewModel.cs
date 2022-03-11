using System.Drawing;
using System.Windows.Input;
using ImageLaka.ImageEngine;
using ImageLaka.Managers;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;

namespace ImageLaka.ViewModels;

public class WorkbenchViewModel : ObservableRecipient
{
    private readonly IDialogService _dialogService;
    private readonly Func<ImageWindowViewModel> _imageVmFactory;

    public Dictionary<string, ImageWindowViewModel> ImageVmMap { get; set; } = new();

    public WorkbenchViewModel(IDialogService dialogService, Func<ImageWindowViewModel> imageVmFactory)
    {
        _dialogService = dialogService;
        _imageVmFactory = imageVmFactory;
    }

    #region 依赖属性

    private string? _imageFiles;

    public string? ImageFiles
    {
        get => _imageFiles;
        set => SetProperty(ref _imageFiles, value);
    }
    public Rectangle Rectangle { get; set; }

    #endregion

    #region 命令

    public ICommand NewImageFileCommand => new RelayCommand(NewImageFile);
    public ICommand OpenImageFileCommand => new RelayCommand(OpenImageFile);
    public ICommand To8BitCommand => new RelayCommand(To8Bit);
    public ICommand To16BitCommand => new RelayCommand(To16Bit);
    public ICommand To32BitCommand => new RelayCommand(To32Bit);
    public ICommand SwitchLanguageCommand => new RelayCommand(SwitchLanguage);
    public ICommand ViewAppLogCommand => new RelayCommand(ViewAppLog);

    private void To8Bit()
    {
        if (ImageFiles == null)
            return;
        ImageWindowViewModel vm;
        if (ImageVmMap.TryGetValue(ImageFiles, out vm))
        {
            vm.To8Bit();
        }
    }

    private void To16Bit()
    {
        if (ImageFiles == null)
            return;
        ImageWindowViewModel vm;
        if (ImageVmMap.TryGetValue(ImageFiles, out vm))
        {
            vm.To16Bit();
        }
    }

    private void To32Bit()
    {
        if (ImageFiles == null)
            return;
        ImageWindowViewModel vm;
        if (ImageVmMap.TryGetValue(ImageFiles, out vm))
        {
            vm.To32Bit();
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
            var files = settings.FileNames;
            foreach (var file in files)
            {
                ImageWindowViewModel vm;
                if (ImageVmMap.ContainsKey(file))
                {
                    vm = ImageVmMap[file];
                }
                else
                {
                    vm = _imageVmFactory.Invoke();
                    vm.Read(file);
                    vm.SetParentWindowBound(Rectangle);
                    ImageVmMap.Add(file, vm);
                }

                _dialogService.Show(this, vm);
                ImageFiles = file;
            }
        }
    }

    private void SwitchLanguage()
    {

    }

    private void ViewAppLog()
    {
    }

    #endregion

}