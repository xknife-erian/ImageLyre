using System.Drawing;
using System.Windows.Input;
using ImageLaka.ImageEngine;
using ImageLaka.ImageEngine.Enums;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using NLog;

namespace ImageLaka.ViewModels;

public class WorkbenchViewModel : ObservableRecipient
{
    private static readonly ILogger _Log = LogManager.GetCurrentClassLogger();
    private readonly IDialogService _dialogService;
    private readonly Func<ImageWindowViewModel> _imageVmFactory;
    private readonly LoggerWindowViewModel _loggerVm;

    public WorkbenchViewModel(IDialogService dialogService, Func<ImageWindowViewModel> imageVmFactory,
        LoggerWindowViewModel loggerVm)
    {
        _dialogService = dialogService;
        _imageVmFactory = imageVmFactory;
        _loggerVm = loggerVm;
    }

    public Dictionary<string, ImageWindowViewModel> ImageVmMap { get; } = new();

    #region 属性

    /// <summary>
    ///     自身窗体大小与位置。当窗体SizeChanged时，会更新该值。通过这个值可以调整其他需要弹出的窗体的位置。
    /// </summary>
    public Rectangle SelfRectangle { get; set; }

    #endregion

    #region 被绑定的属性

    private ImageWindowViewModel? _activatedImageViewModel;

    /// <summary>
    ///     激活的Image窗体ViewModel
    /// </summary>
    public ImageWindowViewModel? ActivatedImageViewModel
    {
        get => _activatedImageViewModel;
        set => SetProperty(ref _activatedImageViewModel, value);
    }

    #region 图像格式菜单中被选中的项

    private bool _isGray;
    private bool _isRGB;
    private bool _isHSV;
    private bool _isCMYK;
    private bool _isLab;
    private bool _is8Bit;
    private bool _is16Bit;
    private bool _is32Bit;

    public bool IsGray
    {
        get => _isGray;
        set => SetProperty(ref _isGray, value);
    }

    public bool IsRGB
    {
        get => _isRGB;
        set => SetProperty(ref _isRGB, value);
    }

    public bool IsHSV
    {
        get => _isHSV;
        set => SetProperty(ref _isHSV, value);
    }

    public bool IsCMYK
    {
        get => _isCMYK;
        set => SetProperty(ref _isCMYK, value);
    }

    public bool IsLab
    {
        get => _isLab;
        set => SetProperty(ref _isLab, value);
    }

    public bool Is8Bit
    {
        get => _is8Bit;
        set => SetProperty(ref _is8Bit, value);
    }

    public bool Is16Bit
    {
        get => _is16Bit;
        set => SetProperty(ref _is16Bit, value);
    }

    public bool Is32Bit
    {
        get => _is32Bit;
        set => SetProperty(ref _is32Bit, value);
    }

    private void UpdateImageFormat()
    {
        var vm = ActivatedImageViewModel;
        if (vm == null || vm.Bitmap == null)
            return;
        var bpp = ImageUtil.GetBitsPerPixel(vm.Bitmap);
        var format = ImageUtil.GetImageFormat(vm.Bitmap);
        IsGray = false;
        IsRGB = false;
        IsHSV = false;
        IsCMYK = false;
        IsLab = false;
        Is8Bit = false;
        Is16Bit = false;
        Is32Bit = false;
        switch (bpp)
        {
            case BitsPerPixel.Bit8:
                Is8Bit = true;
                break;
            case BitsPerPixel.Bit16:
                Is16Bit = true;
                break;
            case BitsPerPixel.Bit32:
                Is32Bit = true;
                break;
        }

        switch (format)
        {
            case ImageFormat.RGB:
                IsRGB = true;
                break;
            case ImageFormat.CMYK:
                IsCMYK = true;
                break;
            case ImageFormat.HSV:
                IsHSV = true;
                break;
            case ImageFormat.Gray:
                IsGray = true;
                break;
            case ImageFormat.Lab:
                IsLab = true;
                break;
        }
    }

    #endregion

    #endregion

    #region 菜单功能命令

    public ICommand NewImageFileCommand => new RelayCommand(NewImageFile);
    public ICommand OpenImageFileCommand => new RelayCommand(OpenImageFile);
    public ICommand ToGrayCommand => new RelayCommand(() => {
        if (ActivatedImageViewModel != null)
        {
            ActivatedImageViewModel.ToGray();
            UpdateImageFormat();
        }
    });
    public ICommand ToRGBCommand => new RelayCommand(() => {
        if (ActivatedImageViewModel != null)
        {
            ActivatedImageViewModel.ToRGB();
            UpdateImageFormat();
        }
    });
    public ICommand ToHSVCommand => new RelayCommand(() => {
        if (ActivatedImageViewModel != null)
        {
            ActivatedImageViewModel.ToHSV();
            UpdateImageFormat();
        }
    }); 
    public ICommand ToCMYKCommand => new RelayCommand(() => {
        if (ActivatedImageViewModel != null)
        {
            ActivatedImageViewModel.ToCMYK();
            UpdateImageFormat();
        }
    });
    public ICommand ToLabCommand => new RelayCommand(() => {
        if (ActivatedImageViewModel != null)
        {
            ActivatedImageViewModel.ToLab();
            UpdateImageFormat();
        }
    });

    public ICommand To8BitCommand => new RelayCommand(() =>
    {
        if (ActivatedImageViewModel != null)
        {
            ActivatedImageViewModel.To8Bit();
            UpdateImageFormat();
        }
    });

    public ICommand To16BitCommand => new RelayCommand(() =>
    {
        if (ActivatedImageViewModel != null)
            ActivatedImageViewModel.To16Bit();
    });

    public ICommand To32BitCommand => new RelayCommand(() =>
    {
        if (ActivatedImageViewModel != null)
        {
            ActivatedImageViewModel.To32Bit();
            UpdateImageFormat();
        }
    });

    public ICommand SwitchLanguageCommand => new RelayCommand(SwitchLanguage);
    public ICommand ViewAppLogCommand => new RelayCommand(ViewAppLog);

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
                    vm.SetParentWindowRectangle(SelfRectangle);
                    vm.WindowIsActivated += (s, e) =>
                    {
                        var ivm = s as ImageWindowViewModel;
                        if (!Equals(ActivatedImageViewModel, ivm))
                        {
                            ActivatedImageViewModel = ivm;
                            UpdateImageFormat();
                        }
                    };
                    ImageVmMap.Add(file, vm);
                }

                _dialogService.Show(this, vm);
            }
        }
    }

    private void SwitchLanguage()
    {
    }

    private void ViewAppLog()
    {
        _dialogService.Show(this, _loggerVm);
        _Log.Info("显示日志窗体完成.");
    }

    #endregion
}