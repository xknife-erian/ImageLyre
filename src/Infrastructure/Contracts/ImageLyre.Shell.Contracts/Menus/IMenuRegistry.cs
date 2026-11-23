namespace ImageLyre.Shell.Contracts.Menus;

/// <summary>菜单贡献的注册表：壳渲染菜单，插件提供菜单与菜单项定义。</summary>
/// <remarks>
/// <para>
/// 这是 G2 命令注册表的最小前置：M1 之后先把“菜单结构与标题”从壳里拿出来，命令 Id、快捷键与可用条件
/// 仍留在 G2。壳只负责渲染与排序，不硬编码任何插件的菜单项。
/// </para>
/// <para>
/// 线程与生命周期：注册与读取都允许在任意线程发生，实现需自行加锁；<see cref="Changed"/> 由实现触发，
/// 订阅方（壳）负责切回 UI 线程。<see cref="RegisterMenu"/> 与 <see cref="RegisterItem"/> 返回的句柄
/// 负责撤销本次注册，重复释放幂等；插件停止时应释放自己注册的句柄。
/// </para>
/// <para>
/// 顺序：<see cref="Menus"/> 与 <see cref="GetItems"/> 都按 <c>Order</c> 升序排列，<c>Order</c> 相同时按
/// 注册先后排列；壳按此顺序渲染，不另做二次排序。
/// </para>
/// </remarks>
public interface IMenuRegistry
{
    /// <summary>当前已注册的顶层菜单，按 <c>Order</c> 与注册顺序排列。</summary>
    IReadOnlyList<MenuDefinition> Menus { get; }

    /// <summary>读取指定菜单下的菜单项，按 <c>Order</c> 与注册顺序排列。</summary>
    /// <param name="menuId">顶层菜单标识。</param>
    /// <returns>菜单项列表；菜单不存在时返回空列表而不是 <see langword="null"/>。</returns>
    IReadOnlyList<MenuItemDefinition> GetItems(string menuId);

    /// <summary>注册一个顶层菜单。</summary>
    /// <param name="menu">菜单定义，不可为 <see langword="null"/>。</param>
    /// <returns>撤销注册的句柄；释放后菜单与它下面的菜单项一并移除。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="menu"/> 为 <see langword="null"/>。</exception>
    /// <exception cref="ArgumentException">Id 或 TitleKey 为空白。</exception>
    /// <exception cref="InvalidOperationException">同一 Id 的菜单已注册。</exception>
    IDisposable RegisterMenu(MenuDefinition menu);

    /// <summary>向已注册的菜单注册一个菜单项。</summary>
    /// <param name="item">菜单项定义，不可为 <see langword="null"/>。</param>
    /// <returns>撤销注册的句柄；重复释放幂等。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="item"/> 为 <see langword="null"/>。</exception>
    /// <exception cref="ArgumentException">MenuId、Id 或 TitleKey 为空白。</exception>
    /// <exception cref="InvalidOperationException">目标菜单不存在，或同一菜单下已存在同一 Id 的菜单项。</exception>
    IDisposable RegisterItem(MenuItemDefinition item);

    /// <summary>菜单或菜单项集合发生变化时触发。</summary>
    /// <remarks>实现逐个调用订阅方并隔离异常；订阅方负责退订，壳在窗口释放时退订。</remarks>
    event EventHandler? Changed;
}

/// <summary>一个顶层菜单的定义。</summary>
/// <param name="Id">菜单标识，全局唯一，例如 <c>help</c>；壳不解释其含义。</param>
/// <param name="TitleKey">标题的资源键，由壳通过本地化服务解析。</param>
/// <param name="Order">渲染顺序，升序；相同时按注册先后。</param>
public sealed record MenuDefinition(string Id, string TitleKey, int Order = 0);

/// <summary>一个菜单项的定义。</summary>
/// <param name="MenuId">所属顶层菜单的标识。</param>
/// <param name="Id">菜单项标识，在同一菜单内唯一。</param>
/// <param name="TitleKey">标题的资源键，由壳通过本地化服务解析；语言切换后壳会重新解析。</param>
/// <param name="Order">渲染顺序，升序；相同时按注册先后。</param>
/// <param name="Invoke">点击时执行的动作；为 <see langword="null"/> 时壳把该项渲染为禁用项。</param>
public sealed record MenuItemDefinition(
    string MenuId,
    string Id,
    string TitleKey,
    int Order = 0,
    Func<Task>? Invoke = null);
