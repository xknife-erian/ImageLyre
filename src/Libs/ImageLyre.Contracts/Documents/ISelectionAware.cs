using System.Reactive;

namespace ImageLyre.Contracts.Documents;

/// <summary>
/// 文档能力接口：暴露当前选择/光标上下文。
/// Shell 通过 ActiveDocument 探测（as ISelectionAware）为命令执行（parameter）、
/// 上下文菜单、状态栏选区信息提供数据源。
/// Selection 形状由文档类型定义（文本区间/节点 ID/坐标等），建议为不可变记录。
/// </summary>
public interface ISelectionAware
{
    /// <summary>当前选择上下文（形状由文档类型定义）；null 表示无选择。</summary>
    object? Selection { get; }

    /// <summary>选择变更时推送（光标移动、选区变化均触发）。</summary>
    /// <remarks>触发线程：UI 线程（实现方负责调度）；回放：无（配合快照属性 <see cref="Selection"/> 使用，订阅后仅推后续变更）；hot。</remarks>
    IObservable<Unit> SelectionChanged { get; }
}
