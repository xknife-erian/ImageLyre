using System.Drawing;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using NLog;

namespace ImageLyric.UI.ViewModels;

public class WorkbenchViewModel : ObservableRecipient
{
    private static readonly ILogger _Log = LogManager.GetCurrentClassLogger();

    #region 变量

    private readonly Dictionary<string, ImageViewModel> _imageVmMap = new();

    #endregion

    #region 构造函数

    public WorkbenchViewModel(
        IDialogService dialogService,
        Func<ImageViewModel> imageVmFactory,
        Func<HistogramViewModel> histogramVmFactory,
        LoggerViewModel loggerVm,
        OptionViewModel optionVm)
    {
        _dialogService = dialogService;
        _imageVmFactory = imageVmFactory;
        _histogramVmFactory = histogramVmFactory;
        _loggerVm = loggerVm;
        _optionVm = optionVm;
    }

    #endregion

    #region IoC注入的变量

    private readonly IDialogService _dialogService;
    private readonly Func<ImageViewModel> _imageVmFactory;
    private readonly Func<HistogramViewModel> _histogramVmFactory;
    private readonly LoggerViewModel _loggerVm;
    private readonly OptionViewModel _optionVm;

    #endregion

    #region 属性

    #endregion

    #region 被绑定的属性

    private ImageViewModel? _activatedImageViewModel;
    private Rectangle _selfRectangle;

    /// <summary>
    ///     激活的Image窗体ViewModel
    /// </summary>
    public ImageViewModel? ActivatedImageViewModel
    {
        get => _activatedImageViewModel;
        set => SetProperty(ref _activatedImageViewModel, value);
    }

    /// <summary>
    ///     自身窗体大小与位置。当窗体SizeChanged时，会更新该值。通过这个值可以调整其他需要弹出的窗体的位置。
    /// </summary>
    public Rectangle SelfRectangle
    {
        get => _selfRectangle;
        set => SetProperty(ref _selfRectangle, value);
    }

    #region 图像格式菜单中被选中的项

    private bool _isGray;
    private bool _isRGB;
    private bool _isHSV;
    private bool _isCMYK;
    private bool _isLab;
    private bool _is8Bit;
    private bool _is16Bit;
    private bool _is24Bit;
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

    public bool Is24Bit
    {
        get => _is24Bit;
        set => SetProperty(ref _is24Bit, value);
    }

    public bool Is32Bit
    {
        get => _is32Bit;
        set => SetProperty(ref _is32Bit, value);
    }

    private void UpdateImageFormat()
    {
        var vm = ActivatedImageViewModel;
        if (vm == null)
            return;
        // var bpp = ImageUtil.GetBitsPerPixel(vm.BmpMat);
        // var format = ImageUtil.GetImageFormat(vm.BmpMat);
        // IsGray = false;
        // IsRGB = false;
        // IsHSV = false;
        // IsCMYK = false;
        // IsLab = false;
        // Is8Bit = false;
        // Is16Bit = false;
        // Is24Bit = false;
        // Is32Bit = false;
        // switch (bpp)
        // {
        //     case BitsPerPixel.Bit8:
        //         Is8Bit = true;
        //         break;
        //     case BitsPerPixel.Bit16:
        //         Is16Bit = true;
        //         break;
        //     case BitsPerPixel.Bit24:
        //         Is24Bit = true;
        //         break;
        //     case BitsPerPixel.Bit32:
        //         Is32Bit = true;
        //         break;
        // }
        //
        // switch (format)
        // {
        //     case ImageLadFormat.RGB:
        //         IsRGB = true;
        //         break;
        //     case ImageLadFormat.CMYK:
        //         IsCMYK = true;
        //         break;
        //     case ImageLadFormat.HSV:
        //         IsHSV = true;
        //         break;
        //     case ImageLadFormat.Gray:
        //         IsGray = true;
        //         break;
        //     case ImageLadFormat.Lab:
        //         IsLab = true;
        //         break;
        // }
    }

    #endregion

    #endregion

    #region 菜单功能命令

    #region 开始 -> 文件

    public ICommand NewImageFileCommand => new RelayCommand(() => { });

    public ICommand OpenImageFileCommand => new RelayCommand(OpenImageFile);

    #endregion

    #region 开始 -> 选项

    public ICommand SetOptionCommand => new RelayCommand(() =>
    {
        _optionVm.SetStartLocation(ComputeRightLocation());
        _dialogService.Show(this, _optionVm);
        _Log.Info("显示选项窗体完成.");
    });

    #endregion

    #region 图像 -> 模式

    public ICommand ToGrayCommand => new RelayCommand(() =>
    {
        if (ActivatedImageViewModel != null)
        {
            ActivatedImageViewModel.ToGray();
            UpdateImageFormat();
        }
    });

    public ICommand ToRGBCommand => new RelayCommand(() =>
    {
        if (ActivatedImageViewModel != null)
        {
            ActivatedImageViewModel.ToRGB();
            UpdateImageFormat();
        }
    });

    public ICommand ToHSVCommand => new RelayCommand(() =>
    {
        if (ActivatedImageViewModel != null)
        {
            ActivatedImageViewModel.ToHSV();
            UpdateImageFormat();
        }
    });

    public ICommand ToCMYKCommand => new RelayCommand(() =>
    {
        if (ActivatedImageViewModel != null)
        {
            ActivatedImageViewModel.ToCMYK();
            UpdateImageFormat();
        }
    });

    public ICommand ToLabCommand => new RelayCommand(() =>
    {
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
        {
            ActivatedImageViewModel.To16Bit();
            UpdateImageFormat();
        }
    });

    public ICommand To24BitCommand => new RelayCommand(() =>
    {
        if (ActivatedImageViewModel != null)
        {
            ActivatedImageViewModel.To24Bit();
            UpdateImageFormat();
        }
    });

    public ICommand To32BitCommand => new RelayCommand(() =>
    {
        if (ActivatedImageViewModel != null)
        {
            ActivatedImageViewModel.To32Bit();
            UpdateImageFormat();
        }
    });

    #endregion

    #region 分析 -> 图表

    public ICommand HistogramCommand => new RelayCommand(() =>
    {
        var vm = _histogramVmFactory.Invoke();
        vm.SetStartLocation(ComputeRightLocation());
        _dialogService.Show(this, vm);
    });

    #endregion

    #region 帮助

    public ICommand SwitchLanguageCommand => new RelayCommand(() => { });

    public ICommand ViewAppLogCommand => new RelayCommand(() =>
    {
        _loggerVm.SetStartLocation(ComputeRightLocation());
        _dialogService.Show(this, _loggerVm);
        _Log.Info("显示日志窗体完成.");
    });

    #endregion

    #endregion

    #region 私有方法

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
                ImageViewModel vm;
                if (_imageVmMap.ContainsKey(file))
                {
                    vm = _imageVmMap[file];
                }
                else
                {
                    vm = _imageVmFactory.Invoke();
                    _imageVmMap.Add(file, vm);
                    vm.Read(file);
                    vm.SetStartLocation(ComputeBelowLocation(_imageVmMap.Count));
                    vm.WindowIsActivated += (s, e) =>
                    {
                        var ivm = s as ImageViewModel;
                        if (!Equals(ActivatedImageViewModel, ivm))
                        {
                            ActivatedImageViewModel = ivm;
                            UpdateImageFormat();
                        }
                    };
                }

                _dialogService.Show(this, vm);
            }
        }
    }

    /// <summary>
    ///     计算出在下方显示时的坐标
    /// </summary>
    private Point ComputeBelowLocation(int count = 0)
    {
        var x = SelfRectangle.Top + SelfRectangle.Height + count * 20;
        var y = SelfRectangle.Left + (count - 1) * 20;
        return new Point(x, y);
    }

    /// <summary>
    ///     计算出在右方显示时的坐标
    /// </summary>
    private Point ComputeRightLocation()
    {
        var x = SelfRectangle.Left + SelfRectangle.Width + 3;
        var y = SelfRectangle.Top;
        return new Point(x, y);
    }

    #endregion
}