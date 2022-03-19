using System;
using ImageLad.ViewModels.Utils.NLog;
using ImageLad.Views.Views;
using NLog;
using NLog.Targets;

namespace ImageLad;

[Target(nameof(LoggerWindow))]
public class NlogTarget : Target
{
    private readonly LoggerStack _loggerStack = LoggerStack.Instance;

    protected override void Write(LogEventInfo logEvent)
    {
        try
        {
            _loggerStack.AddLogInfo(logEvent);
        }
        catch (Exception e)
        {
            Console.WriteLine($@"向控件写日志发生异常.{e.Message}{e.StackTrace}");
        }
    }
}