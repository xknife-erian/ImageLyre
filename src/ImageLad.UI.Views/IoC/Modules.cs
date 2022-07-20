using Autofac;
using ImageLad.UI.Views.Dialogs;
using MvvmDialogs;

namespace ImageLad.UI.Views.IoC;

public class Modules : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);
        builder.RegisterType<Workbench>().AsImplementedInterfaces().AsSelf().SingleInstance();
        builder.Register(context => new DialogService(
                dialogTypeLocator: new DialogTypeLocator(),
                frameworkDialogFactory: new CustomFrameworkDialogFactory()))
            .AsImplementedInterfaces().SingleInstance();
    }
}