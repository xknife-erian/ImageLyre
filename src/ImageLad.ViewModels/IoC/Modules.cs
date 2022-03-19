using Autofac;

namespace ImageLad.ViewModels.IoC
{
    public class Modules : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WorkbenchViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<ImageWindowViewModel>().AsSelf();
            builder.RegisterType<LoggerWindowViewModel>().AsSelf().SingleInstance();
        }
    }


}
