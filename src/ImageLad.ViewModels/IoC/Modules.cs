﻿using Autofac;

namespace ImageLad.ViewModels.IoC
{
    public class Modules : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ImageViewModel>().AsSelf();
            builder.RegisterType<ChartViewModel>().AsSelf();

            //以下均单例
            builder.RegisterType<WorkbenchViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<LoggerViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<OptionViewModel>().AsSelf().SingleInstance();
        }
    }


}
