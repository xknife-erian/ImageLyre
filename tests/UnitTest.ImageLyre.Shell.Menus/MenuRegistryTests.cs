using FluentAssertions;
using ImageLyre.Shell.Contracts.Menus;
using ImageLyre.Shell.Menus;
using Xunit;

namespace UnitTest.ImageLyre.Shell.Menus;

/// <summary>菜单注册表的注册、排序、撤销与校验口径。</summary>
public sealed class MenuRegistryTests
{
    private static MenuItemDefinition Item(string menuId, string id, int order = 0, Func<Task>? invoke = null) =>
        new(menuId, id, $"lyre.test.{menuId}.{id}", order, invoke);

    [Fact(DisplayName = "1.注册顶层菜单后按定义原样读回")]
    public void Test001()
    {
        // Arrange
        var registry = new MenuRegistry();
        var definition = new MenuDefinition("help", "lyre.test.help", 10);

        // Act
        using IDisposable handle = registry.RegisterMenu(definition);

        // Assert
        registry.Menus.Should().ContainSingle().Which.Should().Be(definition);
    }

    [Fact(DisplayName = "2.重复注册同一菜单 Id 抛异常并指出 Id")]
    public void Test002()
    {
        // Arrange
        var registry = new MenuRegistry();
        using IDisposable _ = registry.RegisterMenu(new MenuDefinition("help", "lyre.test.help"));

        // Act
        Action act = () => registry.RegisterMenu(new MenuDefinition("help", "lyre.test.help2"));

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*help*");
    }

    [Fact(DisplayName = "3.顶层菜单按 Order 升序、同序按注册先后排列")]
    public void Test003()
    {
        // Arrange
        var registry = new MenuRegistry();

        // Act
        registry.RegisterMenu(new MenuDefinition("third", "lyre.test.third", 20));
        registry.RegisterMenu(new MenuDefinition("first", "lyre.test.first", 10));
        registry.RegisterMenu(new MenuDefinition("second", "lyre.test.second", 10));

        // Assert
        registry.Menus.Select(menu => menu.Id).Should().ContainInOrder("first", "second", "third");
    }

    [Fact(DisplayName = "4.向不存在的菜单注册菜单项抛异常并指出菜单 Id")]
    public void Test004()
    {
        // Arrange
        var registry = new MenuRegistry();

        // Act
        Action act = () => registry.RegisterItem(Item("missing", "about"));

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*missing*");
    }

    [Fact(DisplayName = "5.同一菜单下重复菜单项 Id 抛异常并指出 Id")]
    public void Test005()
    {
        // Arrange
        var registry = new MenuRegistry();
        registry.RegisterMenu(new MenuDefinition("help", "lyre.test.help"));
        registry.RegisterItem(Item("help", "about"));

        // Act
        Action act = () => registry.RegisterItem(Item("help", "about"));

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*about*");
    }

    [Fact(DisplayName = "6.不同菜单允许使用相同菜单项 Id")]
    public void Test006()
    {
        // Arrange
        var registry = new MenuRegistry();
        registry.RegisterMenu(new MenuDefinition("help", "lyre.test.help"));
        registry.RegisterMenu(new MenuDefinition("file", "lyre.test.file"));

        // Act
        using IDisposable first = registry.RegisterItem(Item("help", "open"));
        using IDisposable second = registry.RegisterItem(Item("file", "open"));

        // Assert
        registry.GetItems("help").Should().ContainSingle().Which.MenuId.Should().Be("help");
        registry.GetItems("file").Should().ContainSingle().Which.MenuId.Should().Be("file");
    }

    [Fact(DisplayName = "7.菜单项按 Order 升序、同序按注册先后排列")]
    public void Test007()
    {
        // Arrange
        var registry = new MenuRegistry();
        registry.RegisterMenu(new MenuDefinition("help", "lyre.test.help"));

        // Act
        registry.RegisterItem(Item("help", "third", 20));
        registry.RegisterItem(Item("help", "first", 10));
        registry.RegisterItem(Item("help", "second", 10));

        // Assert
        registry.GetItems("help").Select(item => item.Id).Should().ContainInOrder("first", "second", "third");
    }

    [Fact(DisplayName = "8.释放菜单项句柄后移除该项并只通知一次，二次释放不再通知")]
    public void Test008()
    {
        // Arrange
        var registry = new MenuRegistry();
        registry.RegisterMenu(new MenuDefinition("help", "lyre.test.help"));
        IDisposable handle = registry.RegisterItem(Item("help", "about"));
        int changed = 0;
        registry.Changed += (_, _) => changed++;

        // Act
        handle.Dispose();
        handle.Dispose();

        // Assert
        registry.GetItems("help").Should().BeEmpty();
        changed.Should().Be(1);
    }

    [Fact(DisplayName = "9.释放菜单句柄后菜单与其菜单项一并移除并通知一次")]
    public void Test009()
    {
        // Arrange
        var registry = new MenuRegistry();
        IDisposable menu = registry.RegisterMenu(new MenuDefinition("help", "lyre.test.help"));
        registry.RegisterItem(Item("help", "about"));
        int changed = 0;
        registry.Changed += (_, _) => changed++;

        // Act
        menu.Dispose();

        // Assert
        registry.Menus.Should().BeEmpty();
        registry.GetItems("help").Should().BeEmpty();
        changed.Should().Be(1);
    }

    [Fact(DisplayName = "10.空白 Id 与标题键被拒绝且异常带参数名")]
    public void Test010()
    {
        // Arrange
        var registry = new MenuRegistry();
        var blankId = new MenuDefinition(" ", "lyre.test.help");
        var blankTitle = new MenuDefinition("help", " ");

        // Act
        Action idAct = () => registry.RegisterMenu(blankId);
        Action titleAct = () => registry.RegisterMenu(blankTitle);

        // Assert
        idAct.Should().Throw<ArgumentException>().WithParameterName("menu");
        titleAct.Should().Throw<ArgumentException>().WithParameterName("menu");
    }

    [Fact(DisplayName = "11.空引用注册被拒绝且异常带参数名")]
    public void Test011()
    {
        // Arrange
        var registry = new MenuRegistry();

        // Act
        Action menuAct = () => registry.RegisterMenu(null!);
        Action itemAct = () => registry.RegisterItem(null!);

        // Assert
        menuAct.Should().Throw<ArgumentNullException>().WithParameterName("menu");
        itemAct.Should().Throw<ArgumentNullException>().WithParameterName("item");
    }

    [Fact(DisplayName = "12.读取不存在的菜单返回空列表")]
    public void Test012()
    {
        // Arrange
        var registry = new MenuRegistry();

        // Act
        IReadOnlyList<MenuItemDefinition> items = registry.GetItems("missing");

        // Assert
        items.Should().BeEmpty();
    }
}
