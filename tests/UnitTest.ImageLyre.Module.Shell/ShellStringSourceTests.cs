using System.Globalization;
using FluentAssertions;
using ImageLyre.Module.Shell;
using Xunit;

namespace ImageLyre.Module.Shell.Tests;

/// <summary>守住 <see cref="ShellStringSource"/> 的中英文案、键前缀与未命中行为。</summary>
public sealed class ShellStringSourceTests
{
    [Fact(DisplayName = "1.zh-CN 的每个已登记键都返回精确中文文案")]
    public void Test001()
    {
        // Arrange
        var source = new ShellStringSource();
        var culture = new CultureInfo("zh-CN");
        var expected = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["lyre.shell.menu.file"] = "文件",
            ["lyre.shell.menu.languageZh"] = "中文",
            ["lyre.shell.menu.languageEn"] = "English",
            ["lyre.shell.menu.placeholderItem"] = "（菜单项由插件提供）",
            ["lyre.shell.status.coordinate"] = "坐标",
            ["lyre.shell.status.pixel"] = "像素值",
            ["lyre.shell.status.depth"] = "位深",
            ["lyre.shell.status.zoom"] = "缩放",
            ["lyre.shell.status.calibration"] = "校准单位",
            ["lyre.shell.status.ready"] = "就绪",
            ["lyre.shell.viewport.empty"] = "无图像",
            ["lyre.shell.panel.empty"] = "尚无测量",
            ["lyre.shell.window.title"] = "ImageLyre 工作台",
        };

        // Act & Assert
        foreach ((string key, string text) in expected)
        {
            bool found = source.TryGet(key, culture, out string value);
            found.Should().BeTrue("中文源应命中键 {0}", key);
            value.Should().Be(text);
        }
    }

    [Fact(DisplayName = "2.en-US 的每个已登记键都返回精确英文文案")]
    public void Test002()
    {
        // Arrange
        var source = new ShellStringSource();
        var culture = new CultureInfo("en-US");
        var expected = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["lyre.shell.menu.file"] = "File",
            ["lyre.shell.menu.languageZh"] = "中文",
            ["lyre.shell.menu.languageEn"] = "English",
            ["lyre.shell.menu.placeholderItem"] = "(Menu items supplied by plugins)",
            ["lyre.shell.status.coordinate"] = "Coordinates",
            ["lyre.shell.status.pixel"] = "Pixel value",
            ["lyre.shell.status.depth"] = "Bit depth",
            ["lyre.shell.status.zoom"] = "Zoom",
            ["lyre.shell.status.calibration"] = "Calibration",
            ["lyre.shell.status.ready"] = "Ready",
            ["lyre.shell.viewport.empty"] = "No image",
            ["lyre.shell.panel.empty"] = "No measurements",
            ["lyre.shell.window.title"] = "ImageLyre Workbench",
        };

        // Act & Assert
        foreach ((string key, string text) in expected)
        {
            bool found = source.TryGet(key, culture, out string value);
            found.Should().BeTrue("英文源应命中键 {0}", key);
            value.Should().Be(text);
        }
    }

    [Fact(DisplayName = "3.未知键返回 false，且只给出空串而不写回退文案")]
    public void Test003()
    {
        // Arrange
        var source = new ShellStringSource();

        // Act
        bool found = source.TryGet("lyre.shell.unknown", new CultureInfo("zh-CN"), out string value);

        // Assert
        found.Should().BeFalse();
        value.Should().BeEmpty("未命中时源只给出空串，不把键名或回退文案写进 out");
    }

    [Fact(DisplayName = "4.同一窗口标题键在 zh-CN 与 en-US 返回不同文案")]
    public void Test004()
    {
        // Arrange
        var source = new ShellStringSource();

        // Act
        bool zhFound = source.TryGet("lyre.shell.window.title", new CultureInfo("zh-CN"), out string zhValue);
        bool enFound = source.TryGet("lyre.shell.window.title", new CultureInfo("en-US"), out string enValue);

        // Assert
        zhFound.Should().BeTrue();
        enFound.Should().BeTrue();
        zhValue.Should().Be("ImageLyre 工作台");
        enValue.Should().Be("ImageLyre Workbench");
        zhValue.Should().NotBe(enValue);
    }

    [Fact(DisplayName = "5.全部登记键都以 lyre.shell. 为前缀且数量为 13")]
    public void Test005()
    {
        // Act & Assert
        ShellStringKeys.All.Should().HaveCount(13);
        ShellStringKeys.All.Should().OnlyContain(
            key => key.StartsWith("lyre.shell.", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "6.旧菜单与语言按钮键已删除，新菜单键都已登记")]
    public void Test006()
    {
        // Arrange
        var source = new ShellStringSource();
        string[] newKeys =
        [
            "lyre.shell.menu.file",
            "lyre.shell.menu.languageZh",
            "lyre.shell.menu.languageEn",
            "lyre.shell.menu.placeholderItem",
        ];
        string[] oldKeys =
        [
            "lyre.shell.menu.placeholder",
            "lyre.shell.language.zh",
            "lyre.shell.language.en",
        ];

        // Act & Assert
        foreach (string key in newKeys)
            ShellStringKeys.All.Should().Contain(key);

        foreach (string key in oldKeys)
        {
            ShellStringKeys.All.Should().NotContain(key);
            bool found = source.TryGet(key, new CultureInfo("zh-CN"), out _);
            found.Should().BeFalse("旧键 {0} 已删除", key);
        }
    }
}
