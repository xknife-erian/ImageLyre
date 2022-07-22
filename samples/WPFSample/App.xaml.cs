using System;
using System.Windows;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using ImageLad.UI.Views.Utils;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace WPFSample
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            Workbench = Ioc.Default.GetRequiredService<WorkbenchViewModel>();
        }
        public WorkbenchViewModel Workbench { get; set; }
    }

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            DispatcherHelper.Initialize();
            Ioc.Default.ConfigureServices(IocSetup());
        }

        private IServiceProvider IocSetup()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<Modules>();
            return new AutofacServiceProvider(builder.Build());
        }
    }
}
