namespace ImageLyre.Contracts.Documents;

/// <summary>
/// 提供对“最近使用文档”的访问能力。
/// 最近使用列表按使用时间倒序排列（索引 0 为最新使用的文档）。
/// 通常在每次激活文档时更新该列表（去重并置顶）。
/// </summary>
public interface IMostRecentlyUsedDocuments
{
    /// <summary>
    /// 获取最近使用的文档 ContentId 列表（最多保留 N 项，如 20）。
    /// 返回值为只读快照，调用方不应修改。
    /// </summary>
    IReadOnlyList<string> MostRecentlyUsed { get; }

    /// <summary>
    /// 记录一次文档激活（宿主在激活文档时调用）：去重并置顶。
    /// 列表变化时经 <see cref="MostRecentlyUsedChanged"/> 推送。
    /// </summary>
    /// <param name="contentId">被激活文档的 ContentId。</param>
    void RecordActivation(string contentId);

    /// <summary>
    /// 最近使用列表变更的响应式流。
    /// 每当 MRU 列表发生变化（如新文档激活、文档关闭），推送当前快照。
    /// </summary>
    /// <remarks>
    /// 触发线程：UI 线程（实现方负责调度）；回放：无（配合快照属性 <see cref="MostRecentlyUsed"/> 使用，订阅后仅推后续变更）；hot。
    /// </remarks>
    IObservable<IReadOnlyList<string>> MostRecentlyUsedChanged { get; }
}
