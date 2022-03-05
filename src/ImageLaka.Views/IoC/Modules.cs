using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using ImageLaka.Views.Dialogs;
using MvvmDialogs;

namespace ImageLaka.Views.IoC
{
    public class Modules : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder.RegisterType<Workbench>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.Register(context => new DialogService(
                    dialogTypeLocator:new DialogTypeLocator(),
                    frameworkDialogFactory: new CustomFrameworkDialogFactory()))
                .AsImplementedInterfaces().SingleInstance();
        }
    }


}
