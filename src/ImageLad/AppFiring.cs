using ImageLad.ViewModels;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace ImageLad;

/// <summary>
///     This class contains static references to all the view models in the
///     application and provides an entry point for the bindings.
/// </summary>
public class AppFiring
{
    public WorkbenchViewModel MainWindow => Ioc.Default.GetRequiredService<WorkbenchViewModel>();
}