using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using ImageLaka.Managers;
using ImageLaka.ViewModels;

namespace ImageLaka.Views;

/// <summary>
///     Interaction logic for Workbench.xaml
/// </summary>
public partial class Workbench
{
    private readonly OptionManager _optionManager;

    public Workbench(OptionManager optionManager)
    {
        _optionManager = optionManager;

        InitializeComponent();
        var vm = (WorkbenchViewModel)DataContext;

        var top = _optionManager.HabitData.GetValue($"{nameof(Workbench)}.{nameof(Top)}", 100);
        Top = int.Parse(top);
        var left = _optionManager.HabitData.GetValue($"{nameof(Workbench)}.{nameof(Left)}", 100);
        Left = int.Parse(left);

        SizeChanged += (s, e) =>
        {
            var width = (int) e.NewSize.Width;
            var height = (int) e.NewSize.Height;
            vm.WindowRectangle = new Rectangle((int) Left, (int) Top, width, height);
        };
        Loaded += delegate
        {
            vm.PropertyChanged += ViewModelPropertyChanged;
        };
        LocationChanged += delegate
        {
            _optionManager.HabitData.SetValue($"{nameof(Workbench)}.{nameof(Top)}", Top);
            _optionManager.HabitData.SetValue($"{nameof(Workbench)}.{nameof(Left)}", Left);
        };

        _Ribbon_.SelectedTabIndex = 0;
    }

    private void ViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender == null)
            return;
        var vm = (WorkbenchViewModel) sender;
    }
}