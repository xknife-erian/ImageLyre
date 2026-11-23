using Cangming.PicoPlugins.Core;
using Cangming.PicoPlugins.Core.Features;
using Cangming.PicoPlugins.Core.Support;
using ImageLyre.Contracts;
using ImageLyre.Shell.Contracts.Menus;

namespace ImageLyre.Feature.About;

/// <summary>关于 Feature 自己的上下文：保存 IoC 访问器并给出本地化与菜单注册表的取用入口。</summary>
/// <remarks>
/// 两个服务都由宿主在容器里注册，Feature 只经 <see cref="ISupportUsingIoC"/> 取得访问器；
/// 上下文不外传，也不在构造期求值。
/// </remarks>
internal sealed class AboutFeatureContext : BaseFeatureContext
{
    private Lazy<IIoCManager>? _ioc;

    /// <summary>接收宿主容器访问器的延迟引用。</summary>
    /// <param name="ioc">IoC 访问器，不得为 <see langword="null"/>。</param>
    public void SetIoC(Lazy<IIoCManager> ioc)
    {
        ArgumentNullException.ThrowIfNull(ioc);
        _ioc = ioc;
    }

    /// <summary>宿主注册的本地化服务。</summary>
    /// <exception cref="InvalidOperationException">IoC 未注入或容器中没有该服务。</exception>
    public ILocalizationService Localization => _ioc?.Value.Resolve<ILocalizationService>()
        ?? throw new InvalidOperationException(
            "宿主未注册 ILocalizationService：关于 Feature 无法取得本地化服务。");

    /// <summary>宿主注册的菜单注册表。</summary>
    /// <exception cref="InvalidOperationException">IoC 未注入或容器中没有该服务。</exception>
    public IMenuRegistry Menus => _ioc?.Value.Resolve<IMenuRegistry>()
        ?? throw new InvalidOperationException(
            "宿主未注册 IMenuRegistry：关于 Feature 无法注册帮助菜单。");
}
