using Autofac;
using ImageLad.Views.Dialogs;
using MvvmDialogs;

namespace ImageLad.Views.IoC;

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