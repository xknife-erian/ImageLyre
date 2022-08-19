using System;
using ImageLyre.NLog;
using ImageLyre.UI.Views.Views;
using NLog;
using NLog.Targets;

namespace ImageLyre;

[Target(nameof(LoggerWindow))]
public class NlogTarget : Target
{
    private readonly LogStack _logStack = LogStack.Instance;

    protected override void Write(LogEventInfo logEvent)
    {
        try
        {
            _logStack.AddLog(logEvent);
        }
        catch (Exception e)
        {
            Console.WriteLine($@"向控件写日志发生异常.{e.Message}{e.StackTrace}");
        }
    }
}