namespace ImageLyre.UI.Contracts.Files;

/// <summary>
///     跨平台文件与文件夹选择服务。
///     封装系统级文件打开、保存及文件夹浏览功能，屏蔽底层平台差异（Windows/macOS/Linux/Web）。
/// </summary>
/// <remarks>
///     <para>
///         - 所有方法均为 <see cref="Task" /> 异步形式，可在任意线程（包括后台线程）安全调用；
///         - 实现方负责将 UI 操作调度至主线程；
///         - 若用户取消操作，方法返回 <see langword="null" />；
///         - 建议在 ViewModel 中通过依赖注入使用，便于单元测试（可 Mock 返回预设路径）。
///     </para>
/// </remarks>
public interface IFileDialogManager
{
    /// <summary>
    ///     异步显示“打开文件”对话框，支持单选或多选。
    /// </summary>
    /// <param name="settings">文件打开配置参数。</param>
    /// <returns>
    ///     用户选中的文件路径列表；若用户取消操作，则返回 <see langword="null" />。
    ///     即使只选择一个文件，也返回单元素列表。
    /// </returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="settings" /> 为 <see langword="null" /> 时抛出。</exception>
    Task<IReadOnlyList<string>?> OpenFilesAsync(FileOpenSettings settings);

    /// <summary>
    ///     异步显示“保存文件”对话框。
    /// </summary>
    /// <param name="settings">文件保存配置参数。</param>
    /// <returns>
    ///     用户指定的完整文件保存路径；若用户取消操作，则返回 <see langword="null" />。
    /// </returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="settings" /> 为 <see langword="null" /> 时抛出。</exception>
    Task<string?> SaveFileAsync(FileSaveSettings settings);

    /// <summary>
    ///     异步显示“选择文件夹”对话框。
    /// </summary>
    /// <param name="settings">文件夹选择配置参数。</param>
    /// <returns>
    ///     用户选择的文件夹完整路径；若用户取消操作，则返回 <see langword="null" />。
    /// </returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="settings" /> 为 <see langword="null" /> 时抛出。</exception>
    Task<string?> SelectFolderAsync(FolderSelectSettings settings);
}

/// <summary>
///     表示一个文件类型过滤器，用于限制用户可见的文件类型。
/// </summary>
/// <example>
///     new FileFilter("文本文件", "*.txt")
///     new FileFilter("图像文件", "*.png;*.jpg;*.jpeg")
///     new FileFilter("所有文件", "*.*")
/// </example>
public record FileFilter
{
    /// <summary>
    ///     过滤器显示名称（如“图像文件”）。
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     文件扩展名模式，支持分号分隔多个（如 "*.png;*.jpg"）。
    /// </summary>
    public required string Pattern { get; init; }
}

/// <summary>
///     已知系统文件夹的语义化枚举，用于跨平台初始化目录。
///     实现方应将其映射到各平台的实际路径（如 Windows 的 %USERPROFILE%\Documents）。
/// </summary>
public enum KnownFolder
{
    /// <summary>当前用户的桌面目录。</summary>
    Desktop,

    /// <summary>当前用户的“文档”目录。</summary>
    Documents,

    /// <summary>默认下载目录（如浏览器下载位置）。</summary>
    Downloads,

    /// <summary>用户主目录（Linux/macOS 的 ~，Windows 的 %USERPROFILE%）。</summary>
    Home,

    /// <summary>系统临时目录。</summary>
    Temp
}

/// <summary>
///     “打开文件”对话框的配置参数。
/// </summary>
public record FileOpenSettings
{
    /// <summary>
    ///     对话框标题（如“打开项目文件”）。
    /// </summary>
    public string Title { get; init; } = "打开";

    /// <summary>
    ///     初始显示的目录路径。
    ///     若同时设置了 <see cref="InitialKnownFolder" />，则优先使用后者。
    /// </summary>
    public string InitialDirectory { get; init; } = string.Empty;

    /// <summary>
    ///     初始显示的已知系统文件夹（如 <see cref="KnownFolder.Documents" />）。
    ///     若设置，则覆盖 <see cref="InitialDirectory" />。
    /// </summary>
    public KnownFolder? InitialKnownFolder { get; init; }

    /// <summary>
    ///     是否允许多选文件。
    ///     默认：<see langword="false" />（单选）。
    /// </summary>
    public bool Multiselect { get; init; }

    /// <summary>
    ///     文件类型过滤器列表（如 [("文本", "*.txt"), ("所有", "*.*")]）。
    ///     若为空或 <see langword="null" />，则显示所有文件。
    /// </summary>
    public IReadOnlyList<FileFilter>? Filters { get; init; }

    /// <summary>
    ///     当用户输入无扩展名的文件名时，是否自动添加默认扩展名。
    ///     默认：<see langword="true" />。
    /// </summary>
    public bool AddExtension { get; init; } = true;

    /// <summary>
    ///     默认文件扩展名（如 "txt"），用于自动补全。
    ///     仅当 <see cref="AddExtension" /> 为 <see langword="true" /> 时生效。
    /// </summary>
    public string DefaultExtension { get; init; } = string.Empty;

    /// <summary>
    ///     是否检查用户选择的文件是否存在（打开时）。
    ///     默认：<see langword="true" />。
    /// </summary>
    public bool CheckFileExists { get; init; } = true;

    /// <summary>
    ///     是否在对话框中显示“以只读方式打开”复选框。
    ///     默认：<see langword="false" />。
    /// </summary>
    public bool ShowReadOnly { get; init; }

    /// <summary>
    ///     “以只读方式打开”复选框的初始状态。
    ///     仅当 <see cref="ShowReadOnly" /> 为 <see langword="true" /> 时有效。
    /// </summary>
    public bool ReadOnlyChecked { get; init; }

    /// <summary>
    ///     （保存对话框专用）是否提示用户确认创建新文件。
    ///     默认：<see langword="false" />。
    /// </summary>
    public bool CreatePrompt { get; init; }

    /// <summary>
    ///     （保存对话框专用）是否在覆盖现有文件时弹出确认提示。
    ///     默认：<see langword="true" />。
    /// </summary>
    public bool OverwritePrompt { get; init; } = true;
}

/// <summary>
///     “保存文件”对话框的配置参数，继承自 <see cref="FileOpenSettings" />。
/// </summary>
public record FileSaveSettings : FileOpenSettings
{
    /// <summary>
    ///     建议的默认文件名（如 "report.txt"）。
    ///     对话框打开时会预填此名称。
    /// </summary>
    public string SuggestedFileName { get; init; } = string.Empty;
}

/// <summary>
///     “选择文件夹”对话框的配置参数。
/// </summary>
public record FolderSelectSettings
{
    /// <summary>
    ///     对话框标题（如“选择输出目录”）。
    /// </summary>
    public string Title { get; init; } = "选择文件夹";

    /// <summary>
    ///     初始显示的目录路径。
    ///     若同时设置了 <see cref="InitialKnownFolder" />，则优先使用后者。
    /// </summary>
    public string InitialDirectory { get; init; } = string.Empty;

    /// <summary>
    ///     初始显示的已知系统文件夹（如 <see cref="KnownFolder.Downloads" />）。
    ///     若设置，则覆盖 <see cref="InitialDirectory" />。
    /// </summary>
    public KnownFolder? InitialKnownFolder { get; init; }

    /// <summary>
    ///     对话框中显示的描述性提示文本（位于树形控件上方）。
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    ///     是否显示“新建文件夹”按钮。
    ///     默认：<see langword="true" />。
    /// </summary>
    public bool ShowNewFolderButton { get; init; } = true;
}
