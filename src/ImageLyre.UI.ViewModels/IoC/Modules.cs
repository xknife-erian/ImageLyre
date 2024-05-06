using Autofac;

namespace ImageLyre.UI.ViewModels.IoC
{
    public class Modules : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ImageViewModel>().AsSelf();
            builder.RegisterType<HistogramViewModel>().AsSelf();

            //以下均单例
            builder.RegisterType<WorkbenchViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<LoggerViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<OptionViewModel>().AsSelf().SingleInstance();
        }
    }


}
