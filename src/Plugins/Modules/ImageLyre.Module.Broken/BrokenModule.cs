using Cangming.PicoPlugins;
using Cangming.PicoPlugins.Core.Modules;

namespace ImageLyre.Module.Broken;

/// <summary>M1 的故障样本：构造期故意抛异常，用于验证插件管理器的失败隔离。</summary>
/// <remarks>
/// 本类型不属于产品功能，不提供任何契约；插件管理器构造它时必然失败，并把清单条目替换为
/// <c>BrokenPlugin</c> 占位，其余插件继续启动。只应在 Debug 输出中出现，Release 归并由仓库根
/// <c>PluginOutputMerge.targets</c> 排除。
/// </remarks>
[Plugin(id: "lyre.test.broken", name: "故障样本")]
public sealed class BrokenModule : BasePicoModule
{
    /// <summary>创建故障样本并立即抛出，构造函数没有返回路径。</summary>
    /// <exception cref="InvalidOperationException">总是抛出，消息说明这是故意制造的故障样本。</exception>
    public BrokenModule() : base()
    {
        throw new InvalidOperationException(
            "这是故意抛出的故障样本：ImageLyre.Module.Broken 只用于验证构造失败隔离，不参与产品功能，Release 输出也不应包含它。");
    }
}
