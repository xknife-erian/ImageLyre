using System.Drawing;
using NLog;

namespace ImageLad.ViewModels.Utils.NLog;

public class Log
{
    private LogEventInfo _logEventInfo;

    public Log(LogEventInfo logEventInfo)
    {
        _logEventInfo = logEventInfo;

        ToolTip = logEventInfo.FormattedMessage;
        Level = logEventInfo.Level.ToString();
        FormattedMessage = logEventInfo.FormattedMessage;
        if (logEventInfo.Exception != null) 
            Exception = logEventInfo.Exception;
        if (logEventInfo.LoggerName != null)
            LoggerName = logEventInfo.LoggerName;
        Time = logEventInfo.TimeStamp.ToString("HH:mm:ss fff");

        SetupColors(logEventInfo);
    }

    public string Time { get; }
    public string LoggerName { get; }
    public string Level { get; }
    public string FormattedMessage { get; }
    public Exception Exception { get; }
    public string ToolTip { get; }
    public Brush Background { get; private set; }
    public Brush Foreground { get; private set; }
    public Brush BackgroundMouseOver { get; private set; }
    public Brush ForegroundMouseOver { get; private set; }

    private void SetupColors(LogEventInfo logEventInfo)
    {
        if (logEventInfo.Level == LogLevel.Warn)
        {
            Background = Brushes.Yellow;
            BackgroundMouseOver = Brushes.GreenYellow;
        }
        else if (logEventInfo.Level == LogLevel.Error)
        {
            Background = Brushes.Tomato;
            BackgroundMouseOver = Brushes.IndianRed;
        }
        else
        {
            Background = Brushes.White;
            BackgroundMouseOver = Brushes.LightGray;
        }

        Foreground = Brushes.Black;
        ForegroundMouseOver = Brushes.Black;
    }
}