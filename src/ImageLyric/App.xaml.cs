using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using ImageLyric.IoC;
using ImageLyric.Managers;
using ImageLyric.NLog;
using ImageLyric.UI.ViewModels;
using ImageLyric.UI.Views;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using NLog;

namespace ImageLyric
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
        private static readonly ILogger _Log = LogManager.GetCurrentClassLogger();

        public App()
        {
            Ioc.Default.ConfigureServices(IocSetup());

            var president = Ioc.Default.GetService<President>();
            if (president == null)
                return;
            president.OptionManager.Initialize();
            president.ConsoleManager.Initianize();
            UI.Views.Utils.UI.DispatcherInitialize();
            LogStack.UIDispatcher = UI.Views.Utils.UI.CheckBeginInvokeOnUI;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (IsStartFromCommandLine(e))
            {
                var args = e.Args;//这是命令行参数。 TODO:命令行模式待开发。2022/3/7。
                _Log.Info(args);
            }
            else
            {
                var logo = new Welcome();
                logo.Loaded += (_, _) =>
                {
                    var shell = Ioc.Default.GetService<Workbench>();
                    if (shell == null)
                        return;
                    _Log.Info("beginning...");
                    shell.Loaded += async (_, _) =>
                    {
                        await Task.Delay(12 * 100);
                        logo.Close();
                        _Log.Trace("关闭欢迎窗体。");
                    };
                    shell.Closing += OnWorkbenchClosing;
                    shell.Closed += OnWorkbenchClosed;
                    shell.Show();
                    _Log.Info("主窗体显示成功。");
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
            _Log.Info("OnWorkbenchClosing...");
        }

        private void OnWorkbenchClosed(object? sender, EventArgs e)
        {
            _Log.Info("OnWorkbenchClosed.");
            Environment.Exit(0);
        }

        private static IServiceProvider IocSetup()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<Modules>();
            builder.RegisterModule<ImageLyric.Services.Macros.IoC.Modules>();
            builder.RegisterModule<ImageLyric.UI.Views.IoC.Modules>();
            builder.RegisterModule<ImageLyric.UI.ViewModels.IoC.Modules>();
            return new AutofacServiceProvider(builder.Build());
        }
    }
}
