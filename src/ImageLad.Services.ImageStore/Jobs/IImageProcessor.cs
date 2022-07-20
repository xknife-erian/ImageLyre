namespace ImageLad.ImageEngine.Jobs;

/// <summary>
/// 图像处理器
/// </summary>
public interface IImageProcessor
{
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="jobName">处理器名称</param>
    /// <param name="srcBufferGetter">获取图像源的方法</param>
    /// <param name="setting">处理器设置</param>
    void Initialize(string jobName, Func<byte[]> srcBufferGetter, Action<byte[]> bufferSetter, ImageProcessSetting setting);

    /// <summary>
    /// 开始处理
    /// </summary>
    void Start();
    /// <summary>
    /// 停止处理。工作线程将被释放。
    /// </summary>
    void Stop();
    /// <summary>
    /// 暂停处理。工作线程将被保留，与<see cref="Resume"/>方法对应。
    /// </summary>
    void Pause();
    /// <summary>
    /// 恢复处理。与<see cref="Pause"/>方法对应。
    /// </summary>
    void Resume();
}