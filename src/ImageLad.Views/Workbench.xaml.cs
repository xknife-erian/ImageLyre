using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using ImageLad.Managers;
using ImageLad.ViewModels;
using NKnife;
using NLog;

namespace ImageLad.Views;

/// <summary>
///     Interaction logic for Workbench.xaml
/// </summary>
public partial class Workbench
{
    private static readonly ILogger _Log = LogManager.GetCurrentClassLogger();

    private readonly OptionManager _optionManager;

    public Workbench(OptionManager optionManager)
    {
        _optionManager = optionManager;

        InitializeComponent();
        var vm = (WorkbenchViewModel)DataContext;

        if (_optionManager.HabitData == null)
        {
            _Log.Warn("用户习惯载入失败.");
            _optionManager.HabitData = new HabitData();
        }
        var top = _optionManager.HabitData.GetValue($"{nameof(Workbench)}.{nameof(Top)}", 100);
        Top = int.Parse(top);
        var left = _optionManager.HabitData.GetValue($"{nameof(Workbench)}.{nameof(Left)}", 100);
        Left = int.Parse(left);

        Loaded += (s, e) =>
        {
            vm.PropertyChanged += ViewModelPropertyChanged;
            vm.SelfRectangle = new Rectangle((int)Left, (int)Top, (int)Width, (int)Height);
        };
        LocationChanged += (s, e) =>
        {
            _optionManager.HabitData.SetValue($"{nameof(Workbench)}.{nameof(Top)}", Top);
            _optionManager.HabitData.SetValue($"{nameof(Workbench)}.{nameof(Left)}", Left);
            vm.SelfRectangle = new Rectangle((int)Left, (int)Top, (int)Width, (int)Height);
        };
        SizeChanged += (s, e) =>
        {
            var width = (int)e.NewSize.Width;
            var height = (int)e.NewSize.Height;
            vm.SelfRectangle = new Rectangle((int)Left, (int)Top, width, height);
        };
        _Ribbon_.SelectedTabIndex = 0;
    }

    private void ViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender == null)
            return;
        var vm = (WorkbenchViewModel) sender;
        switch (e.PropertyName)
        {
            case nameof(vm.ActivatedImageViewModel):
                if (vm.ActivatedImageViewModel != null)
                    _Ribbon_.SelectedTabIndex = 1;
                break;
        }
    }
}