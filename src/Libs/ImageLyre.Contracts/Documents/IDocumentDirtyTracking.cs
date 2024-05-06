
namespace ImageLyre.Contracts.Documents;

/// <summary>
/// Shell 聚合查询角色：以 contentId 为参数提供文档脏状态的聚合查询
/// （单文档判定 <see cref="IsDocumentDirty"/>、脏文档批量列表 <see cref="GetDirtyDocuments"/>、
/// 全局脏状态变更流 <see cref="DocumentDirtyStateChanged"/>）。
///
/// 文档实例自报保存能力是 <c>ISaveCommands</c>（如 <c>SaveAsync</c> 的 Task&lt;bool&gt; 语义），
/// 与本接口的切分线 = contentId 参数化：本接口只做按 contentId 的查询/观察，不负责保存；
/// 带副作用的保存-关闭编排（脏检测 → 保存提示 → 关闭/放弃/取消）由
/// <see cref="IDocumentManager.TryCloseWithPrompt"/> 统一承担。
/// </summary>
public interface IDocumentDirtyTracking
{
    /// <summary>
    /// 判断指定文档是否处于“脏”状态（即内容已修改但未保存）。
    /// </summary>
    /// <param name="contentId">文档的唯一标识符</param>
    /// <returns>若为脏状态返回 true，否则 false</returns>
    bool IsDocumentDirty(string contentId);

    /// <summary>
    /// 获取当前所有处于脏状态的文档 ID 列表。
    /// </summary>
    /// <returns>脏文档 ID 的只读列表</returns>
    IReadOnlyList<string> GetDirtyDocuments();

    /// <summary>
    /// 当任意文档的脏状态发生变化时触发。
    /// </summary>
    /// <returns>可观察流，包含 (文档ID, 是否脏） 元组</returns>
    /// <remarks>
    /// 触发线程：UI 线程（实现方负责调度）；回放：无（配合快照查询 <see cref="IsDocumentDirty"/> / <see cref="GetDirtyDocuments"/> 使用，订阅后仅推后续变更）；hot。
    /// </remarks>
    IObservable<(string ContentId, bool IsDirty)> DocumentDirtyStateChanged { get; }
}

/// <summary>
/// 文档关闭操作的结果类型。
/// </summary>
public enum DocumentCloseResult
{
    /// <summary>文档已成功关闭。</summary>
    Closed,

    /// <summary>用户取消了关闭操作（如点击“取消”按钮）。</summary>
    Canceled,

    /// <summary>用户选择保存，但在保存过程中发生错误导致关闭失败。</summary>
    SaveFailed
}
