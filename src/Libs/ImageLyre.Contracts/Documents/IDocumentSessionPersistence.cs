namespace ImageLyre.Contracts.Documents;

/// <summary>
/// 文档会话持久化服务：负责在应用退出时保存工作区状态，并在启动时恢复。
/// </summary>
public interface IDocumentSessionPersistence
{
    /// <summary>
    /// 异步保存当前会话状态到磁盘。
    /// </summary>
    Task SaveAsync();

    /// <summary>
    /// 异步从磁盘恢复上次会话状态。
    /// </summary>
    Task RestoreAsync();
}

/// <summary>
/// 表示应用程序会话状态的数据模型，用于保存和恢复工作区。
/// </summary>
/// <remarks>
/// 反序列化兼容性：此类型经 System.Text.Json 序列化持久化。
/// record 的 init-only 属性可由 System.Text.Json 反射设置（.NET 5+ 支持 init 访问器）；
/// <see cref="IReadOnlyList{T}"/> 属性在反序列化时由 System.Text.Json 内部物化为
/// List&lt;T&gt; 再赋给只读接口属性（无需自定义转换器）。
/// </remarks>
public record DocumentSession
{
    /// <summary>
    /// 启动时需要重新打开的文档 ID 列表。
    /// </summary>
    public IReadOnlyList<string> OpenDocuments { get; init; } = new List<string>();

    /// <summary>
    /// 应激活的文档 ID（通常是上次关闭前正在编辑的文档）。
    /// </summary>
    public string? ActiveDocument { get; init; }

    /// <summary>
    /// 最近使用文档列表，用于 MRU 功能恢复。
    /// </summary>
    public IReadOnlyList<string> MruList { get; init; } = new List<string>();

    // 可扩展字段：如分组信息、窗口布局、展开节点等
}
