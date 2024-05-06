namespace ImageLyre.UI.Contracts.UserState;
/// <summary>
/// 用户状态记忆。这是一个轻量级、自动化的状态记忆机制，不是让用户去“设置”的东西。
/// 自动记录和恢复用户在使用软件过程中产生的隐式行为状态，用于优化体验。
/// 例如：主窗口位置与大小、上次激活的文档、最近浏览路径、折叠的面板等。
/// 这些数据通常不由用户显式设置，而是由系统自动捕获并用于提升连续性体验。
/// </summary>
public interface IUserStateMemory
{
    /// <summary>
    /// 获取指定记忆项的值。若未记录，返回默认值。
    /// </summary>
    T GetValue<T>(string key, T defaultValue = default!);

    /// <summary>
    /// 记录一个记忆项的当前值。
    /// 系统将在适当时机（如退出前）自动持久化，或可手动触发保存。
    /// </summary>
    void RecordValue<T>(string key, T value);

    /// <summary>
    /// 显式将所有已记录的记忆数据持久化到磁盘。
    /// 建议在应用退出前调用。
    /// </summary>
    void Save();

    /// <summary>
    /// 当某个记忆项被更新时推送其 key（可用于实时响应，如调试）。
    /// </summary>
    /// <remarks>
    /// 触发线程：UI 线程（实现方负责调度）；回放：无（配合快照查询 GetValue 使用，订阅后仅推后续变更）；hot。
    /// </remarks>
    IObservable<string> ValueUpdated { get; }
}
