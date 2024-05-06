using ImageLyre.Contracts.Documents;
using System.Reactive;
using ImageLyre.UI.Contracts.Commands;

namespace ImageLyre.UI.Contracts.Documents;

/// <summary>
/// 文档窗格管理器：负责管理已打开的文档及其激活状态。
/// 
/// Document 代表用户的核心工作内容（如代码文件、设计画布、数据表等），
/// 与辅助性的 Rail（属性面板、日志等）形成主-辅分离架构。
/// 
/// ⚠️ 设计原则：
/// - 所有方法线程安全，可在任意线程调用（实现需调度到 UI 线程）。
/// - 每个文档由唯一 ContentId 标识（来自 <see cref="IHasContentId.ContentId"/>）。
/// - 文档关闭前的未保存变更编排由 <see cref="TryCloseWithPrompt"/> 统一承担
///   （脏检测 → 保存提示 → 关闭/放弃/取消）；实际保存逻辑由文档自身的
///   <see cref="ISaveCommands"/> 提供，本接口不实现保存。
/// </summary>
public interface IDocumentManager
{
#region 📄 核心命令（副作用）

    /// <summary>
    /// 显示或激活指定的文档 ViewModel。
    /// 若文档尚未打开，则添加并显示；若已打开，则激活。
    /// </summary>
    /// <param name="documentViewModel">文档的 ViewModel，必须实现 <see cref="IDocumentViewModel"/>。</param>
    /// <param name="options">文档行为选项。若为 null，则使用默认选项。</param>
    /// <exception cref="ArgumentNullException">documentViewModel 为 null。</exception>
    /// <remarks>
    /// <b>方法返回时副作用已生效</b>（文档已打开或激活）；任意线程调用时实现方须内部调度并等待完成，
    /// 不得返回后异步生效。
    /// </remarks>
    void Show(IDocumentViewModel documentViewModel, DocumentOptions? options = null);

    /// <summary>
    /// 尝试关闭指定文档；若文档处于脏状态，先弹出保存提示（保存/放弃/取消）。
    /// 返回 <see cref="DocumentCloseResult"/>（Closed/Canceled/SaveFailed）。
    /// </summary>
    /// <param name="contentId">文档的唯一标识符。</param>
    /// <returns>
    /// <see cref="DocumentCloseResult.Closed"/>：文档已成功关闭；  
    /// <see cref="DocumentCloseResult.Canceled"/>：用户取消关闭（含放弃后取消）；  
    /// <see cref="DocumentCloseResult.SaveFailed"/>：用户选择保存但保存失败，文档保持打开。
    /// </returns>
    Task<DocumentCloseResult> TryCloseWithPrompt(string contentId);

    /// <summary>
    /// 尝试重新激活指定 ContentId 的文档。
    /// </summary>
    /// <param name="contentId">文档的唯一标识符。</param>
    /// <returns>
    /// <see langword="true"/>：成功激活已有文档；  
    /// <see langword="false"/>：文档不存在；  
    /// <see langword="null"/>：已是当前激活文档（无变化）。
    /// </returns>
    /// <exception cref="ArgumentException">contentId 为 null 或空。</exception>
    bool? Reopen(string contentId);

    /// <summary>
    /// 关闭指定 ContentId 的文档。
    /// 此操作仅销毁文档视图，**不负责保存内容**——调用方应在关闭前确认保存。
    /// </summary>
    /// <param name="contentId">文档的唯一标识符。</param>
    /// <returns><see langword="true"/> 如果文档存在并被成功关闭；否则 <see langword="false"/>。</returns>
    bool Close(string contentId);

#endregion
#region 👁️ 当前状态（只读快照）

    /// <summary>
    /// 当前激活的文档 ViewModel（可能为 null）。
    /// </summary>
    IDocumentViewModel? ActiveDocument { get; }

    /// <summary>
    /// 当前激活文档的 ContentId（null 表示无激活文档）。
    /// </summary>
    string? ActiveContentId { get; }

    /// <summary>
    /// 当前激活文档的只读元组，便于 XAML 绑定。
    /// </summary>
    (string? ContentId, IDocumentViewModel? ViewModel) ActiveDocumentTuple { get; }

    /// <summary>
    /// 所有当前已打开的文档 ViewModel 列表（只读快照）。
    /// </summary>
    IReadOnlyList<IDocumentViewModel> OpenDocuments { get; }

    /// <summary>
    /// 检查指定文档是否当前已打开。
    /// </summary>
    /// <param name="contentId">文档的唯一标识符。</param>
    /// <returns><see langword="true"/> 如果文档已打开；否则 <see langword="false"/>。</returns>
    bool IsDocumentOpen(string contentId);

#endregion
#region 🔁 状态变更流（Reactive）

    /// <summary>
    /// 激活文档变更的响应式流。
    /// 每次激活文档变化时推送新的 (contentId, viewModel) 元组。
    /// </summary>
    /// <remarks>
    /// 触发线程：UI 线程（实现方负责调度）；回放：无（配合快照属性 <see cref="ActiveDocument"/> / <see cref="ActiveContentId"/> 使用，订阅后仅推后续变更）；hot。
    /// </remarks>
    IObservable<(string? ContentId, IDocumentViewModel? ViewModel)> ActiveDocumentChanged { get; }

    /// <summary>
    /// 已打开文档集合变更的响应式流（新增、关闭均触发）。
    /// </summary>
    /// <remarks>
    /// 触发线程：UI 线程（实现方负责调度）；回放：无（配合快照属性 <see cref="OpenDocuments"/> 使用，订阅后仅推后续变更）；hot。
    /// </remarks>
    IObservable<Unit> OpenDocumentsChanged { get; }

#endregion
}
