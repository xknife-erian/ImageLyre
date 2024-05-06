using ImageLyre.Contracts.Documents;
namespace ImageLyre.UI.Contracts.Documents;
/// <summary>
/// 文档的行为与显示选项。
/// 可在 <see cref="IDocumentManager.Show"/> 时传入，或由 ViewModel 默认提供。
/// </summary>
public record DocumentOptions
{
    /// <summary>
    /// 是否允许同一 ContentId 的文档被多次打开（多实例模式）。
    /// 默认：false（单实例，重复 Show 会激活已有实例）。
    /// </summary>
    public bool AllowMultipleInstances { get; init; }

    /// <summary>
    /// 是否允许用户关闭此文档（通过关闭按钮或快捷键）。
    /// 默认：true。
    /// </summary>
    public bool CanClose { get; init; } = true;

    /// <summary>
    /// 是否将此文档的标签页固定（通常表现为不可关闭 + 置顶显示）。
    /// 若为 true，则隐式设置 CanClose = false。
    /// 默认：false。
    /// </summary>
    public bool Pinned { get; init; }

    /// <summary>
    /// 是否参与布局持久化（<see cref="IDocumentManager.SaveLayout"/>）。
    /// 若为 false，则每次启动都按默认行为处理（如临时预览文档）。
    /// 默认：true。
    /// </summary>
    public bool PersistLayout { get; init; } = true;

    /// <summary>
    /// 自定义图标资源键（用于 XAML 中的 StaticResource 引用）。
    /// 示例："IconCodeFile"。
    /// 默认：null（使用默认文档图标）。
    /// </summary>
    public string? IconResourceKey { get; init; }

    /// <summary>
    /// 在恢复布局时，若文档无法打开（如文件丢失），是否静默跳过。
    /// 默认：true（避免阻塞整个布局恢复）。
    /// </summary>
    public bool SkipOnRestoreFailure { get; init; } = true;
}