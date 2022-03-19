using Autofac;
using ImageLad.Managers;

namespace ImageLad.Base.IoC
{
    public class Modules : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<President>().AsSelf().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<OptionManager>().AsSelf().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ConsoleManager>().AsSelf().AsImplementedInterfaces().SingleInstance();
        }
    }


}
