using System.Drawing;
using System.Drawing.Imaging;
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

    #region 属性

    /// <summary>
    /// Window的大小与位置
    /// </summary>
    public Rectangle WindowRectangle { get; set; }
    
    #endregion

    #region 被绑定的属性

    private ImageWindowViewModel? _activatedImageViewModel;

    /// <summary>
    /// 激活的Image窗体ViewModel
    /// </summary>
    public ImageWindowViewModel? ActivatedImageViewModel
    {
        get => _activatedImageViewModel;
        set => SetProperty(ref _activatedImageViewModel, value);
    }

    #endregion

    #region 菜单功能命令

    public ICommand NewImageFileCommand => new RelayCommand(NewImageFile);
    public ICommand OpenImageFileCommand => new RelayCommand(OpenImageFile);
    public ICommand To8BitCommand => new RelayCommand(To8Bit);
    public ICommand To16BitCommand => new RelayCommand(To16Bit);
    public ICommand To32BitCommand => new RelayCommand(To32Bit);
    public ICommand SwitchLanguageCommand => new RelayCommand(SwitchLanguage);
    public ICommand ViewAppLogCommand => new RelayCommand(ViewAppLog);

    private void To8Bit()
    {
        if (ActivatedImageViewModel != null)
            ActivatedImageViewModel.To8Bit();
    }

    private void To16Bit()
    {
        if (ActivatedImageViewModel != null)
            ActivatedImageViewModel.To16Bit();
    }
    private void To32Bit()
    {
        if (ActivatedImageViewModel != null)
            ActivatedImageViewModel.To32Bit();
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
            Multiselect = true,
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
                    vm.SetParentWindowBound(WindowRectangle);
                    ImageVmMap.Add(file, vm);
                }
                ActivatedImageViewModel = vm;
                ActivatedImageViewModel.WindowIsActivated += (s, e) =>
                {
                    ActivatedImageViewModel = s as ImageWindowViewModel;
                };
                _dialogService.Show(this, vm);
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