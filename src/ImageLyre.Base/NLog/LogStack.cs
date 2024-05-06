using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using NLog;

namespace ImageLyre.NLog;

public sealed class LogStack
{
    private const int MaxViewCount = 200;

    /// <summary>
    ///     供NLog组件快速吐出日志的线程安全集合。
    /// </summary>
    private readonly ConcurrentStack<Log> _stack = new();

    private bool _isRun = true;

    private LogStack()
    {
        //每5毫秒从日志Stack中读取弹出的日志，以便于海量日志不会在毫秒级内发生阻塞。
        Task.Factory.StartNew(() =>
        {
            while (_isRun)
            {
                if (!_stack.IsEmpty)
                {
                    var infos = new Log[_stack.Count];
                    _stack.TryPopRange(infos); //弹出Stack中的所有的日志
                    UIDispatcher?.Invoke(() =>
                    {
                        foreach (var log in infos)
                            Logs.Insert(0, log); //触发ListView显示最新的日志
                        SizeCollection(Logs, MaxViewCount);
                    });
                    Thread.Sleep(5);
                }
            }
        });
    }
    
    /// <summary>
    /// 被界面绑定的集合
    /// </summary>
    public ObservableCollection<Log> Logs { get; } = new();

    /// <summary>
    ///     添加日志
    /// </summary>
    /// <param name="logEvent"></param>
    public void AddLog(LogEventInfo logEvent)
    {
        _stack.Push(new Log(logEvent));
    }

    /// <summary>
    ///     将指定的集合缩减到指定大小
    /// </summary>
    /// <param name="collection">指定的集合</param>
    /// <param name="size">指定大小</param>
    public static void SizeCollection<T>(IList<T> collection, int size)
    {
        if (collection.Count >= size)
            for (var i = 0; i < collection.Count - size; i++)
                collection.RemoveAt(collection.Count - 1);
    }

    #region 单件实例

    /// <summary>
    ///     获得一个本类型的单件实例.
    /// </summary>
    /// <value>The instance.</value>
    public static LogStack Instance => _MyInstance.Value;

    private static readonly Lazy<LogStack> _MyInstance = new(() => new LogStack());
    
    public static Action<Action>? UIDispatcher { get; set; }

    #endregion
}