using System.Collections.ObjectModel;
using System.Drawing;
using ImageLyre.NLog;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ImageLyre.UI.ViewModels;

public class LoggerViewModel : ObservableRecipient
{
    public LoggerViewModel()
    {
        var logStack = LogStack.Instance;
        Logs = logStack.Logs;
    }

    public ObservableCollection<Log> Logs { get; }

    public double TimeWidth { get; set; } = 120;

    public double LoggerNameWidth { get; set; } = 50;

    public double LevelWidth { get; set; } = 50;

    public double MessageWidth { get; set; } = 200;

    public double ExceptionWidth { get; set; } = 0;

    public int MaxRowCount { get; set; } = 50;

    public string TimeHeader { get; set; } = "时间";

    public string LoggerNameHeader { get; set; } = "发生";

    public string LevelHeader { get; set; } = "等级";

    public string MessageHeader { get; set; } = "信息";

    public string ExceptionHeader { get; set; } = "异常";

    public Point Location { get; private set; }

    public void SetStartLocation(Point location)
    {
        Location = location;
    }
}