namespace ImageLyre.UI.Contracts.Slots;

/// <summary>
///     槽位承载的内容形态，决定附件类型与渲染器之间的匹配关系。
/// </summary>
/// <remarks>
///     渲染层只认自己词汇的附件；类型不匹配时管理器在收口报告里记一条 <see cref="SlotIssueKind.KindMismatch" />，
///     而不是让附件悄悄消失。
/// </remarks>
public enum SlotKind
{
    /// <summary>菜单树。</summary>
    Menu,

    /// <summary>工具栏按钮。</summary>
    Toolbar,

    /// <summary>状态栏文本或指示器。</summary>
    StatusBar,

    /// <summary>任意界面内容。</summary>
    Content,

    /// <summary>文档页。</summary>
    Document,

    /// <summary>工具窗格。</summary>
    ToolArea,
}

/// <summary>
///     槽位容量：单值槽只接受第一个附件，后到的被拒绝并记诊断。
/// </summary>
public enum SlotCapacity
{
    /// <summary>可挂任意多个附件。</summary>
    Multiple,

    /// <summary>只允许一个附件，先到者胜。</summary>
    Exclusive,
}
