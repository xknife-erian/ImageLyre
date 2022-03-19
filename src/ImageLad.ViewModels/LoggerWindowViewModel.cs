using System.Collections.ObjectModel;
using System.Drawing;
using ImageLad.ViewModels.Utils.NLog;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using NKnife.Events;

namespace ImageLad.ViewModels;

public class LoggerWindowViewModel : ObservableRecipient
{
    private static Action<Action>? _dispatcherAction;

    public LoggerWindowViewModel()
    {
        var loggerStack = LoggerStack.Instance;
        loggerStack.LogAdded += LoggerStackOnLogAdded;
    }

    public ObservableCollection<Log> Logs { get; set; } = new();

    public Point Location { get; set; }

    public double TimeWidth { get; set; } = 120;

    public double LoggerNameWidth { get; set; } = 50;

    public double LevelWidth { get; set; } = 50;

    public double MessageWidth { get; set; } = 200;

    public double ExceptionWidth { get; set; } = 0;

    public int MaxRowCount { get; set; } = 50;

    public bool AutoScrollToLast { get; set; } = true;

    public string TimeHeader { get; set; } = "Time";

    public string LoggerNameHeader { get; set; } = "Logger";

    public string LevelHeader { get; set; } = "Level";

    public string MessageHeader { get; set; } = "Message";

    public string ExceptionHeader { get; set; } = "Exception";

    public static void SetDispatcher(Action<Action> checkBeginInvokeOnUi)
    {
        _dispatcherAction = checkBeginInvokeOnUi;
    }

    private void LoggerStackOnLogAdded(object? sender, EventArgs<Log[]> e)
    {
        _dispatcherAction?.Invoke(() =>
        {
            foreach (var log in e.Item)
                Logs.Insert(0, log);
        });
    }

    public void SetStartLocation(Point location)
    {
        Location = location;
    }
}