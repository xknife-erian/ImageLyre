using ImageLyre.Shell.Contracts.Menus;

namespace ImageLyre.Shell.Menus;

/// <summary>菜单注册表的默认实现：内存登记、按 Order 与注册先后排序、句柄撤销。</summary>
/// <remarks>
/// <para>
/// 线程安全：所有读写都在 <c>_gate</c> 内完成，读取返回快照；<see cref="Changed"/> 在锁外逐个调用并吞掉
/// 订阅方异常，避免一个坏订阅方让注册失败或让其它订阅方漏收通知。
/// </para>
/// <para>
/// 排序：集合按 <c>(Order, Seq)</c> 升序，<c>Seq</c> 是进程内单调递增的注册序号，因此同 Order 时表现为注册先后；
/// 撤销后重新注册会拿到新序号，排到同 Order 的末尾，这是“重新注册视为新条目”的直接结果。
/// </para>
/// </remarks>
public sealed class MenuRegistry : IMenuRegistry
{
    private readonly Lock _gate = new();
    private readonly List<MenuEntry> _menus = [];
    private long _nextSequence;

    /// <inheritdoc />
    public IReadOnlyList<MenuDefinition> Menus
    {
        get
        {
            lock (_gate)
            {
                return _menus
                       .OrderBy(entry => entry.Definition.Order)
                       .ThenBy(entry => entry.Sequence)
                       .Select(entry => entry.Definition)
                       .ToArray();
            }
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<MenuItemDefinition> GetItems(string menuId)
    {
        lock (_gate)
        {
            MenuEntry? entry = FindMenu(menuId);
            return entry is null
                       ? []
                       : entry.Items
                              .OrderBy(item => item.Definition.Order)
                              .ThenBy(item => item.Sequence)
                              .Select(item => item.Definition)
                              .ToArray();
        }
    }

    /// <inheritdoc />
    public IDisposable RegisterMenu(MenuDefinition menu)
    {
        ArgumentNullException.ThrowIfNull(menu, nameof(menu));
        ThrowIfBlank(menu.Id, nameof(menu), "菜单 Id 不能为空白。");
        ThrowIfBlank(menu.TitleKey, nameof(menu), "菜单标题键不能为空白。");

        lock (_gate)
        {
            if (FindMenu(menu.Id) is not null)
                throw new InvalidOperationException($"菜单 '{menu.Id}' 已注册，不能重复注册。");

            _menus.Add(new MenuEntry(menu, _nextSequence++));
        }

        RaiseChanged();
        return new Registration(() => RemoveMenu(menu.Id), menu.Id);
    }

    /// <inheritdoc />
    public IDisposable RegisterItem(MenuItemDefinition item)
    {
        ArgumentNullException.ThrowIfNull(item, nameof(item));
        ThrowIfBlank(item.MenuId, nameof(item), "菜单项所属菜单 Id 不能为空白。");
        ThrowIfBlank(item.Id, nameof(item), "菜单项 Id 不能为空白。");
        ThrowIfBlank(item.TitleKey, nameof(item), "菜单项标题键不能为空白。");

        lock (_gate)
        {
            MenuEntry menu = FindMenu(item.MenuId)
                             ?? throw new InvalidOperationException(
                                 $"菜单 '{item.MenuId}' 尚未注册，菜单项 '{item.Id}' 无处挂载。");

            if (menu.Items.Any(candidate => candidate.Definition.Id == item.Id))
                throw new InvalidOperationException(
                    $"菜单 '{item.MenuId}' 下已存在 Id 为 '{item.Id}' 的菜单项。");

            menu.Items.Add(new ItemEntry(item, _nextSequence++));
        }

        RaiseChanged();
        return new Registration(() => RemoveItem(item.MenuId, item.Id), item.Id);
    }

    /// <inheritdoc />
    public event EventHandler? Changed;

    private void RemoveMenu(string menuId)
    {
        bool removed;

        lock (_gate)
        {
            int index = _menus.FindIndex(entry => entry.Definition.Id == menuId);
            removed = index >= 0;

            if (removed)
                _menus.RemoveAt(index);
        }

        if (removed)
            RaiseChanged();
    }

    private void RemoveItem(string menuId, string itemId)
    {
        bool removed = false;

        lock (_gate)
        {
            MenuEntry? menu = FindMenu(menuId);

            if (menu is not null)
            {
                int index = menu.Items.FindIndex(entry => entry.Definition.Id == itemId);
                removed = index >= 0;

                if (removed)
                    menu.Items.RemoveAt(index);
            }
        }

        if (removed)
            RaiseChanged();
    }

    private MenuEntry? FindMenu(string menuId) =>
        _menus.FirstOrDefault(entry => entry.Definition.Id == menuId);

    private void RaiseChanged()
    {
        EventHandler? handlers = Changed;

        if (handlers is null)
            return;

        foreach (EventHandler handler in handlers.GetInvocationList().Cast<EventHandler>())
        {
            try
            {
                handler(this, EventArgs.Empty);
            }
            catch
            {
                // 订阅方异常不得让注册失败，也不得让其它订阅方漏收通知。
            }
        }
    }

    private static void ThrowIfBlank(string value, string parameterName, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(message, parameterName);
    }

    private sealed class MenuEntry(MenuDefinition definition, long sequence)
    {
        public MenuDefinition Definition { get; } = definition;
        public long Sequence { get; } = sequence;
        public List<ItemEntry> Items { get; } = [];
    }

    private sealed class ItemEntry(MenuItemDefinition definition, long sequence)
    {
        public MenuItemDefinition Definition { get; } = definition;
        public long Sequence { get; } = sequence;
    }

    private sealed class Registration(Action dispose, string name) : IDisposable
    {
        private Action? _dispose = dispose;

        public void Dispose()
        {
            Action? action = Interlocked.Exchange(ref _dispose, null);

            if (action is null)
                return;

            try
            {
                action();
            }
            catch (InvalidOperationException)
            {
                // 撤销是尽力而为：目标已被更早的撤销移除时不再抛出。
            }
        }

        public override string ToString() => $"菜单注册句柄 {name}";
    }
}
