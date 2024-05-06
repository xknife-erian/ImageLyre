
namespace ImageLyre.Contracts.Messaging
{
    /// <summary>
    /// 全局事件聚合器（发布-订阅模式）。
    /// 允许模块间松耦合通信，避免直接依赖事件源。
    /// 适用于跨模块通知场景，如语言切换、主题变更、应用退出等。
    /// </summary>
    public interface IEventAggregator
    {
        /// <summary>
        /// 订阅指定类型的事件。
        /// 返回的 <see cref="IDisposable"/> 可用于取消订阅（推荐在组件销毁时调用）。
        /// </summary>
        /// <typeparam name="TEvent">事件类型，必须为引用类型且非 null。</typeparam>
        /// <param name="handler">事件处理委托。</param>
        /// <returns>可用于取消订阅的句柄。</returns>
        IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : notnull;

        /// <summary>
        /// 发布一个事件到所有当前订阅者。
        /// 事件在调用线程同步分发（通常应在 UI 线程调用）。
        /// </summary>
        /// <typeparam name="TEvent">事件类型。</typeparam>
        /// <param name="eventMessage">事件实例。</param>
        void Publish<TEvent>(TEvent eventMessage) where TEvent : notnull;
    }
}
