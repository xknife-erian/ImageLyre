using System.Reactive;
using ImageLyre.Contracts.Diagnostics;

namespace ImageLyre.UI.Contracts.Output;

/// <summary>
/// 输出面板管理器：多通道文本输出（对标 VS Code OutputChannel / Rider Build Output）。
/// 与诊断条目（<see cref="IDiagnosticAggregator"/>）严格区分：
/// 本接口是自由文本流（追加式、无结构），诊断是结构化条目（有严重度/位置/代码）。
///
/// ⚠️ 设计原则：
/// - 通道按 id 唯一寻址；同名通道只允许一个实例，重复 GetOrCreate 返回同一通道。
/// - 所有方法线程安全，可在任意线程调用（实现需调度到 UI 线程）。
/// </summary>
public interface IOutputChannelManager
{
    /// <summary>获取或创建通道；已存在同 id 则返回现有通道。</summary>
    /// <param name="id">通道唯一标识。</param>
    /// <param name="name">通道显示名称。</param>
    /// <returns>已存在或新建的通道。</returns>
    /// <exception cref="ArgumentException">id 或 name 为 null 或空。</exception>
    IOutputChannel GetOrCreateChannel(string id, string name);

    /// <summary>获取通道；不存在返回 null。</summary>
    /// <param name="id">通道唯一标识。</param>
    /// <returns>匹配的通道；不存在时为 <see langword="null"/>。</returns>
    IOutputChannel? GetChannel(string id);

    /// <summary>移除通道；不存在返回 false。</summary>
    /// <param name="id">通道唯一标识。</param>
    /// <returns>是否移除成功（通道不存在时返回 <see langword="false"/>）。</returns>
    bool RemoveChannel(string id);

    /// <summary>当前全部通道（创建顺序）。</summary>
    IReadOnlyList<IOutputChannel> Channels { get; }

    /// <summary>通道集合变更时推送（创建/移除均触发）。</summary>
    /// <remarks>触发线程：UI 线程（实现方负责调度）；回放：无（配合快照属性 <see cref="Channels"/> 使用，订阅后仅推后续变更）；hot。</remarks>
    IObservable<Unit> ChannelsChanged { get; }
}

/// <summary>单个输出通道（自由文本流）。</summary>
public interface IOutputChannel
{
    /// <summary>通道唯一标识。</summary>
    string Id { get; }

    /// <summary>通道显示名称。</summary>
    string Name { get; }

    /// <summary>追加文本。</summary>
    /// <remarks>线程安全：实现方负责调度到 UI 线程，调用方可在任意线程调用。</remarks>
    /// <param name="text">要追加的文本内容。</param>
    void Append(string text);

    /// <summary>清空通道内容。</summary>
    void Clear();

    /// <summary>聚焦显示该通道（由 Shell 实现决定 UI 形态，如切换到输出面板并选中本通道）。</summary>
    void Show();

    /// <summary>新文本追加时推送（完整追加片段，非历史全量）。</summary>
    /// <remarks>触发线程：UI 线程（实现方负责调度）；回放：无（仅推后续追加）；hot。</remarks>
    IObservable<string> TextAppended { get; }
}
