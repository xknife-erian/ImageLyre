﻿using NLog;

namespace ImageLad.ViewModels.Utils.NLog;

public class NLogEvent : EventArgs
{
    private readonly LogEventInfo _logEventInfo;

    public NLogEvent(LogEventInfo logLogEventInfo)
    {
        _logEventInfo = logLogEventInfo;
    }


    public static implicit operator LogEventInfo(NLogEvent e)
    {
        return e._logEventInfo;
    }

    public static implicit operator NLogEvent(LogEventInfo e)
    {
        return new NLogEvent(e);
    }
}