using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using NKnife.Events;
using NLog;

namespace ImageLad.ViewModels.Utils.NLog;

public sealed class LoggerStack : IDisposable
{
    /// <summary>
    /// 供NLog组件快速吐出日志的线程安全集合。
    /// </summary>
    private readonly ConcurrentStack<Log> _stack = new();

    private uint _maxViewCount = 250;
#if DEBUG
    private Level _currentLevel = Level.Trace | Level.Debug | Level.Info | Level.Warn | Level.Error | Level.Fatal;
#else
        private Level _currentLevel = Level.Info | Level.Warn | Level.Error | Level.Fatal;
#endif

    /// <summary>
    ///     获得一个本类型的单件实例.
    /// </summary>
    /// <value>The instance.</value>
    public static LoggerStack Instance => _MyInstance.Value;

    private static readonly Lazy<LoggerStack> _MyInstance = new(() => new LoggerStack());
    bool isRun = true;

    private LoggerStack()
    {
        //每毫秒从日志Stack中读取弹出的日志，以便于海量日志不会在毫秒级内发生阻塞。
        //考虑到NKnife.NLog.WinForm组件是为了显示，这样的设计不会导致逻辑的问题。
        var thread = new Thread(() =>
        {
            while (isRun)
            {
                if (!_stack.IsEmpty)
                {
                    Log[] infos = new Log[_stack.Count];
                    _stack.TryPopRange(infos); //弹出Stack中的所有的日志
                    //Dispatcher.BeginInvoke(new Action(() => {
                    OnLogAdded(new EventArgs<Log[]>(infos)); //触发ListView显示最新的日志
                    //}));
                    Thread.Sleep(1);
                }
            }
        });
        thread.Name = $"{nameof(LoggerStack)}_ReadLog_Thread";
        thread.IsBackground = true;

        thread.Start();
    }

    /// <summary>
    /// 与界面交互的日志集合，通过绑定集合的“增删改”产生界面的日志显示。
    /// </summary>
    public event EventHandler<EventArgs<Log[]>> LogAdded;

    /// <summary>
    /// 当默认显示的日志数量发生变化时发生
    /// </summary>
    public event EventHandler MaxViewCountChanged;

    /// <summary>
    /// 当需要显示的日志级别枚举发生变化时发生
    /// </summary>
    public event EventHandler CurrentLevelChanged;

    /// <summary>
    /// 需要显示的日志级别
    /// </summary>
    public Level CurrentLevel
    {
        get => _currentLevel;
        set
        {
            _currentLevel = value;
            OnCurrentLevelChanged();
        }
    }


    /// <summary>
    /// 默认显示的日志数量
    /// </summary>
    public uint MaxViewCount
    {
        get => _maxViewCount;
        set
        {
            _maxViewCount = value;
            OnMaxViewCountChanged();
        }
    }

    /// <summary>
    /// 向ViewModel中添加日志
    /// </summary>
    /// <param name="logEvent"></param>
    public void AddLogInfo(LogEventInfo logEvent)
    {
        if (CurrentLevel.HasFlag(GetTopLevel(logEvent.Level)))
        {
            _stack.Push(new Log(logEvent));
        }
    }

    /// <summary>
    /// 将指定的集合缩减到指定大小
    /// </summary>
    /// <param name="collection">指定的集合</param>
    /// <param name="size">指定大小</param>
    public static void SizeCollection<T>(List<T> collection, int size)
    {
        if (collection.Count >= size)
        {
            collection.RemoveRange(size - 1, collection.Count - size);
        }
    }

    private static Level GetTopLevel(LogLevel logLevel)
    {
        if (!Enum.TryParse(logLevel.Name, out Level result))
            result = Level.None;
        return result;
    }

    private void OnMaxViewCountChanged()
    {
        MaxViewCountChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnCurrentLevelChanged()
    {
        CurrentLevelChanged?.Invoke(this, EventArgs.Empty);
    }
    private void OnLogAdded(EventArgs<Log[]> e)
    {
        LogAdded?.Invoke(this, e);
    }

    #region Implementation of IDisposable

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        isRun = false;
        _stack.Clear();
    }

    #endregion
}