using Avalonia.Controls;

namespace ImageLyre.Shell.Contracts;

/// <summary>插件中心契约：插件列表、运行期启停与区域（槽位）清单的查看入口。</summary>
/// <remarks>
///     窗口由插件中心模块创建，宿主与其它功能只认这个契约；窗口内部直接读写沧溟的插件管理器与区域管理器。
/// </remarks>
public interface IPluginCenter
{
    /// <summary>创建插件中心窗口；重复调用返回同一实例。</summary>
    /// <returns>尚未显示的窗口。</returns>
    Window CreateWindow();
}
