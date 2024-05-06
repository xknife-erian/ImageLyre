
namespace ImageLyre.Contracts.Messaging
{
    /// <summary>
    /// 全局异常处理器。
    /// 用于捕获未处理异常、记录日志、弹出友好提示或上报错误。
    /// </summary>
    public interface IGlobalExceptionHandler
    {
        /// <summary>
        /// 注册一个全局异常回调函数。
        /// 可注册多个（如：日志记录器 + UI 提示器）。
        /// </summary>
        /// <param name="handler">异常处理委托。</param>
        /// <returns>注销令牌：Dispose 幂等，Dispose 后该回调不再被调用。</returns>
        IDisposable RegisterHandler(Action<Exception> handler);

        /// <summary>
        /// 主动报告一个非致命异常（例如插件内部错误）。
        /// 不会中断程序，但会被记录和通知。
        /// </summary>
        /// <param name="ex">异常对象。</param>
        /// <param name="context">上下文描述（如 "Plugin X 初始化失败"）。</param>
        void ReportException(Exception ex, string context = "");
    }
}
