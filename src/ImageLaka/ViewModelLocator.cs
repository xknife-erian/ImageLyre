﻿using ImageLaka.ViewModels;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace ImageLaka;

/// <summary>
///     This class contains static references to all the view models in the
///     application and provides an entry point for the bindings.
/// </summary>
public class ViewModelLocator
{
    public WorkbenchViewModel MainWindow => Ioc.Default.GetRequiredService<WorkbenchViewModel>();
}