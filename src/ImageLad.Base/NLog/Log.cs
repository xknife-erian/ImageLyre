using NLog;

namespace ImageLad.NLog;

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
    }

    public string Time { get; }
    public string LoggerName { get; }
    public string Level { get; }
    public string FormattedMessage { get; }
    public Exception Exception { get; }
    public string ToolTip { get; }

}