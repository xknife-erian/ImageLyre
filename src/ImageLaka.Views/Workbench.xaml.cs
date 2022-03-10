using System.ComponentModel;
using ImageLaka.ViewModels;

namespace ImageLaka.Views;

/// <summary>
///     Interaction logic for Workbench.xaml
/// </summary>
public partial class Workbench
{
    public Workbench()
    {
        InitializeComponent();
        Loaded += delegate
        {
            var vm = (WorkbenchViewModel) DataContext;
            vm.PropertyChanged += ViewModelPropertyChanged;
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
            case nameof(vm.ImageFiles):
                if (!string.IsNullOrEmpty(vm.ImageFiles))
                    _Ribbon_.SelectedTabIndex = 1;
                break;
        }
    }
}