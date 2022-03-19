﻿namespace ImageLad.ViewModels.Utils.NLog;

public enum Level : byte
{
    None = 0,
    Trace = 1,
    Debug = 2,
    Info = 4,
    Warn = 8,
    Error = 16,
    Fatal = 32
}