﻿using ImageLad.ImageEngine;
using NLog;

namespace ImageLad.Services.Macros.Beats;

public class To32BitBeat : BaseMacroBeat
{
    private static readonly ILogger _Log = LogManager.GetCurrentClassLogger();

    public To32BitBeat(ITarget target) : base(target)
    {
    }

    protected override bool DoSpecific()
    {
        try
        {
            ImageTarget.To32Bit();
            _Log.Info($"{ImageTarget.File.FullName} To32Bit.");
            return true;
        }
        catch (Exception e)
        {
            _Log.Warn(e);
            return false;
        }
    }

    public override void UnExecute()
    {
        throw new NotImplementedException();
    }
}