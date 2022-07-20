using System;
using System.Threading;

namespace ImageLad.Skills;

public class ImageProcessor : IImageProcessor
{
    private AutoResetEvent _signal = new AutoResetEvent(false);
    private ImageProcessSetting _setting;
    private Action<byte[]> _bufferSetter;
    private Func<byte[]> _srcBufferGetter;
    private Thread _kernelThread;

    #region Implementation of IImageProcessor

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="jobName">处理器名称</param>
    /// <param name="srcBufferGetter">获取图像源的方法</param>
    /// <param name="targetBitmapGetter">设置目标图像的方法</param>
    /// <param name="setting">处理器设置</param>
    public void Initialize(string jobName, Func<byte[]> srcBufferGetter, Action<byte[]> bufferSetter, ImageProcessSetting setting)
    {
        _srcBufferGetter = srcBufferGetter;
        _bufferSetter = bufferSetter;
        _setting = setting;
        InitializeThread(jobName);
    }

    private void InitializeThread(string jobName)
    {
        _kernelThread = new Thread(Run) {IsBackground = true, Name = $"[{jobName}-UI-Processing-Thread]"};
    }

    public bool JobEnable { get; set; } = false;

    /// <summary>
    /// 开始处理
    /// </summary>
    public void Start()
    {
        _kernelThread.Start();
    }

    /// <summary>
    /// 停止处理。工作线程将被释放。
    /// </summary>
    public void Stop()
    {
        JobEnable = false;
        Thread.Sleep(50);
        _kernelThread.Abort();
        _kernelThread = null;
    }

    /// <summary>
    /// 暂停处理。工作线程将被保留，与<see cref="IImageProcessor.Resume"/>方法对应。
    /// </summary>
    public void Pause()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 恢复处理。与<see cref="IImageProcessor.Pause"/>方法对应。
    /// </summary>
    public void Resume()
    {
        throw new NotImplementedException();
    }

    #endregion

    private void Run()
    {
        JobEnable = true;
        while (JobEnable)
        {
            var span = new Span<byte>(_srcBufferGetter());
            var result = RunProcess(span).ToArray();
            _bufferSetter.Invoke(result);
        }
    }

    private Span<byte> RunProcess(Span<byte> span)
    {
        return span;
    }
}