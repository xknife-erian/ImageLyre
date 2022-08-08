using System;
using System.ComponentModel;
using Autofac;
using ImageLad.UI.ViewModels;
using ImageLad.UI.Views.Dialogs;
using MvvmDialogs;
using MvvmDialogs.DialogTypeLocators;
using WPFSample.Panes;

namespace WPFSample;

public class Modules : Module
{
    #region Overrides of Module

    /// <summary>Override to add registrations to the container.</summary>
    /// <remarks>
    ///     Note that the ContainerBuilder parameter is unique to this module.
    /// </remarks>
    /// <param name="builder">
    ///     The builder through which components can be
    ///     registered.
    /// </param>
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);
        builder.Register(context => new DialogService(
                dialogTypeLocator: new DialogTypeLocator(),
                frameworkDialogFactory: new CustomFrameworkDialogFactory()))
            .AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<Workbench>().AsImplementedInterfaces().AsSelf().SingleInstance();
        builder.RegisterType<WorkbenchViewModel>().AsImplementedInterfaces().AsSelf().SingleInstance();
        builder.RegisterType<HistogramSampleViewModel>().AsImplementedInterfaces().AsSelf().SingleInstance();
        builder.RegisterType<MatSampleViewModel>().AsImplementedInterfaces().AsSelf().SingleInstance();
        builder.RegisterType<CamSampleViewModel>().AsImplementedInterfaces().AsSelf().SingleInstance();
    }

    #endregion
}

public class DialogTypeLocator : IDialogTypeLocator
{
    #region Implementation of IDialogTypeLocator

    /// <summary>
    ///     Locates a dialog type based on the specified view model.
    /// </summary>
    public Type Locate(INotifyPropertyChanged viewModel)
    {
        var vmName = viewModel.GetType().Name;
        switch (vmName)
        {
            case nameof(ProgressViewModel):
                return typeof(ProgressDialog);
            default:
                throw new NotImplementedException(vmName);
        }
    }

    #endregion
}