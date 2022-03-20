using System.Collections.ObjectModel;
using System.Drawing;
using ImageLad.ViewModels.Utils.NLog;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using NKnife.Events;

namespace ImageLad.ViewModels;

public class LoggerWindowViewModel : ObservableRecipient
{

    public LoggerWindowViewModel()
    {
        var logStack = LogStack.Instance;
        Logs = logStack.Logs;
    }

    public ObservableCollection<Log> Logs { get; private set; }

    public Point Location { get; set; }

    public double TimeWidth { get; set; } = 120;

    public double LoggerNameWidth { get; set; } = 50;

    public double LevelWidth { get; set; } = 50;

    public double MessageWidth { get; set; } = 200;

    public double ExceptionWidth { get; set; } = 0;

    public int MaxRowCount { get; set; } = 50;

    public string TimeHeader { get; set; } = "Time";

    public string LoggerNameHeader { get; set; } = "Logger";

    public string LevelHeader { get; set; } = "Level";

    public string MessageHeader { get; set; } = "Message";

    public string ExceptionHeader { get; set; } = "Exception";

    public void SetStartLocation(Point location)
    {
        Location = location;
    }
}