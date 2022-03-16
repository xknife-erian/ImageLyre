﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using ImageLaka.Base;
using ImageLaka.Managers;
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
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        public App()
        {
            Ioc.Default.ConfigureServices(IocSetup());

            var president = Ioc.Default.GetService<President>();
            if (president == null)
                return;
            president.OptionManager.Initialize();
            president.ConsoleManager.Initianize();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (IsStartFromCommandLine(e))
            {
                var args = e.Args;//这是命令行参数。 TODO:命令行模式待开发。laka, 2022/3/7。
                Log.Info(args);
            }
            else
            {
                var logo = new Logo();
                logo.Loaded += (_, _) =>
                {
                    var shell = Ioc.Default.GetService<Workbench>();
                    if (shell == null)
                        return;
                    Log.Info("beginning...");
                    shell.Loaded += async (_, _) =>
                    {
                        await Task.Delay(12 * 100);
                        logo.Close();
                        Log.Trace("关闭欢迎窗体。");
                    };
                    shell.Closing += OnWorkbenchClosing;
                    shell.Closed += OnWorkbenchClosed;
                    shell.Show();
                    Log.Info("主窗体显示成功。");
                };
                logo.Show();
            }
        }

        /// <summary>
        /// 判断是否启动为一个命令行模式（无操作窗体）
        /// </summary>
        /// <param name="e">启动参数类</param>
        /// <returns>当true时，以命令行模式启动；反之以标准WPF Window模式启动应用程序</returns>
        private static bool IsStartFromCommandLine(StartupEventArgs e)
        {
            return false; //e.Args.Length > 0;
        }

        private void OnWorkbenchClosing(object? sender, CancelEventArgs e)
        {
            Log.Info("OnWorkbenchClosing...");
        }

        private void OnWorkbenchClosed(object? sender, EventArgs e)
        {
            Log.Info("OnWorkbenchClosed.");
            Environment.Exit(0);
        }

        private static IServiceProvider IocSetup()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<ImageLaka.Base.IoC.Modules>();
            builder.RegisterModule<ImageLaka.ImageEngine.IoC.Modules>();
            builder.RegisterModule<ImageLaka.Services.Macros.IoC.Modules>();
            builder.RegisterModule<ImageLaka.Views.IoC.Modules>();
            builder.RegisterModule<ImageLaka.ViewModels.IoC.Modules>();
            return new AutofacServiceProvider(builder.Build());
        }
    }
}
