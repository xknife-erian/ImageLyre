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
            [ShellStringKeys.MenuPlaceholder] = "菜单",
            [ShellStringKeys.MenuPlaceholderItem] = "（菜单项由插件提供）",
            [ShellStringKeys.StatusCoordinate] = "坐标",
            [ShellStringKeys.StatusPixel] = "像素值",
            [ShellStringKeys.StatusDepth] = "位深",
            [ShellStringKeys.StatusZoom] = "缩放",
            [ShellStringKeys.StatusCalibration] = "校准单位",
            [ShellStringKeys.StatusReady] = "就绪",
            [ShellStringKeys.ViewportEmpty] = "无图像",
            [ShellStringKeys.PanelEmpty] = "尚无测量",
            [ShellStringKeys.WindowTitle] = "ImageLyre 工作台",
            [ShellStringKeys.LanguageZh] = "中文",
            [ShellStringKeys.LanguageEn] = "English",
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
            [ShellStringKeys.MenuPlaceholder] = "Menu",
            [ShellStringKeys.MenuPlaceholderItem] = "(Menu items supplied by plugins)",
            [ShellStringKeys.StatusCoordinate] = "Coordinates",
            [ShellStringKeys.StatusPixel] = "Pixel value",
            [ShellStringKeys.StatusDepth] = "Bit depth",
            [ShellStringKeys.StatusZoom] = "Zoom",
            [ShellStringKeys.StatusCalibration] = "Calibration",
            [ShellStringKeys.StatusReady] = "Ready",
            [ShellStringKeys.ViewportEmpty] = "No image",
            [ShellStringKeys.PanelEmpty] = "No measurements",
            [ShellStringKeys.WindowTitle] = "ImageLyre Workbench",
            [ShellStringKeys.LanguageZh] = "Chinese",
            [ShellStringKeys.LanguageEn] = "English",
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
        bool zhFound = source.TryGet(ShellStringKeys.WindowTitle, new CultureInfo("zh-CN"), out string zhValue);
        bool enFound = source.TryGet(ShellStringKeys.WindowTitle, new CultureInfo("en-US"), out string enValue);

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
}
