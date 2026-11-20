using Avalonia.Controls;

namespace ImageLyre.UI.Contracts.Workbench;

/// <summary>工作台壳契约：插件与宿主之间只通过它交换窗口，互不认识实现类型。</summary>
public interface IWorkbenchShell
{
    /// <summary>创建主窗口；重复调用返回同一实例。</summary>
    /// <returns>尚未显示的主窗口。</returns>
    /// <remarks>只能在界面框架启动后调用：窗口构造会创建控件，提前调用违反"启动期不建控件"的时序约束。</remarks>
    Window CreateShell();

    /// <summary>设置窗口标题。</summary>
    /// <param name="title">新标题；窗口尚未创建时先暂存，创建后一并应用。</param>
    void SetTitle(string title);

    /// <summary>显示忙状态遮罩。</summary>
    /// <param name="message">进度文本。</param>
    /// <param name="progress">0 到 100 的进度值。</param>
    void ShowBusy(string message, uint progress);

    /// <summary>隐藏忙状态遮罩。</summary>
    void HideBusy();
}
