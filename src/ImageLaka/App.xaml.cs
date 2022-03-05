using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using ImageLaka.Views;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using NLog;

namespace ImageLaka
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILogger _Logger = LogManager.GetCurrentClassLogger();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var logo = new Logo();
            logo.Loaded += (_, _) =>
            {
                Ioc.Default.ConfigureServices(IocSetup());

                var shell = Ioc.Default.GetService<Workbench>();
                if (shell == null)
                    return;
                _Logger.Info("beginning...");
                shell.Loaded += async (_, _) =>
                {
                    await Task.Delay(12 * 100);
                    logo.Close();
                    _Logger.Trace("关闭欢迎窗体。");
                };
                shell.Closing += OnWorkbenchClosing;
                shell.Closed += OnWorkbenchClosed;
                shell.Show();
                _Logger.Info("主窗体显示成功。");
            };
            logo.Show();
        }

        private void OnWorkbenchClosing(object? sender, CancelEventArgs e)
        {
            _Logger.Info("OnWorkbenchClosing...");
        }

        private void OnWorkbenchClosed(object? sender, EventArgs e)
        {
            _Logger.Info("OnWorkbenchClosed.");
            Environment.Exit(0);
        }

        private static IServiceProvider IocSetup()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<ImageLaka.Views.IoC.Modules>();
            builder.RegisterModule<ImageLaka.ViewModels.IoC.Modules>();
            return new AutofacServiceProvider(builder.Build());
        }
    }
}
