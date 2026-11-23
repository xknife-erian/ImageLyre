using System.Globalization;
using System.Reflection;
using Cangming.PicoPlugins.Core;
using Cangming.PicoPlugins.Core.Support;
using FluentAssertions;
using ImageLyre.Contracts;
using ImageLyre.Feature.About;
using ImageLyre.Localization;
using ImageLyre.Shell.Contracts.Menus;
using ImageLyre.Shell.Menus;
using Xunit;

namespace UnitTest.ImageLyre.Feature.About;

/// <summary>关于 Feature 的菜单注册与中英双语文案口径。</summary>
public sealed class AboutFeatureTests
{
    private static async Task<(AboutFeature Feature, MenuRegistry Menus)> CreateStartedAsync()
    {
        var localization = new LocalizationService();
        var menus = new MenuRegistry();
        var ioc = new FakeIoC(localization, menus);
        var feature = new AboutFeature();

        await ((ISupportUsingIoC)feature).InjectAsync(new Lazy<IIoCManager>(ioc));
        await feature.StartServiceAsync();

        return (feature, menus);
    }

    [Fact(DisplayName = "1.启动后注册 help 菜单与 about 菜单项")]
    public async Task Test001()
    {
        // Arrange & Act
        (AboutFeature _, MenuRegistry menus) = await CreateStartedAsync();

        // Assert
        menus.Menus.Should().ContainSingle().Which.Should().Be(new MenuDefinition("help", AboutStringKeys.MenuHelp, 100));

        MenuItemDefinition item = menus.GetItems("help").Should().ContainSingle().Subject;
        item.MenuId.Should().Be("help");
        item.Id.Should().Be("about");
        item.TitleKey.Should().Be(AboutStringKeys.MenuAbout);
        item.Order.Should().Be(0);
        item.Invoke.Should().NotBeNull();
    }

    [Fact(DisplayName = "2.停止后帮助菜单与关于项一并撤销")]
    public async Task Test002()
    {
        // Arrange
        (AboutFeature feature, MenuRegistry menus) = await CreateStartedAsync();

        // Act
        await feature.StopServiceAsync();

        // Assert
        menus.Menus.Should().BeEmpty();
        menus.GetItems("help").Should().BeEmpty();
    }

    [Fact(DisplayName = "3.每个关于文案键在中英两套文案里都有非空值")]
    public async Task Test003()
    {
        // Arrange
        (AboutFeature feature, MenuRegistry _) = await CreateStartedAsync();
        string[] keys =
        [
            .. typeof(AboutStringKeys)
               .GetFields(BindingFlags.Public | BindingFlags.Static)
               .Where(field => field.IsLiteral && field.FieldType == typeof(string))
               .Select(field => (string)field.GetRawConstantValue()!),
        ];

        // Act & Assert
        foreach (string key in keys)
        {
            feature.TryGet(key, CultureInfo.GetCultureInfo("zh-CN"), out string zh).Should().BeTrue(key);
            zh.Should().NotBeNullOrWhiteSpace(key);
            feature.TryGet(key, CultureInfo.GetCultureInfo("en-US"), out string en).Should().BeTrue(key);
            en.Should().NotBeNullOrWhiteSpace(key);
            en.Should().NotBe(zh, $"{key} 的英文文案不能与中文相同");
        }
    }

    [Fact(DisplayName = "4.未知键与空参数返回 false 且文案为空")]
    public async Task Test004()
    {
        // Arrange
        (AboutFeature feature, MenuRegistry _) = await CreateStartedAsync();

        // Act
        bool unknown = feature.TryGet("lyre.feature.about.missing", CultureInfo.GetCultureInfo("zh-CN"), out string unknownValue);
        bool nullCulture = feature.TryGet(AboutStringKeys.Close, null!, out string nullCultureValue);

        // Assert
        unknown.Should().BeFalse();
        unknownValue.Should().BeEmpty();
        nullCulture.Should().BeFalse();
        nullCultureValue.Should().BeEmpty();
    }

    [Fact(DisplayName = "5.关闭按钮与版权文案在中英下取到确定值")]
    public async Task Test005()
    {
        // Arrange
        (AboutFeature feature, MenuRegistry _) = await CreateStartedAsync();

        // Act
        feature.TryGet(AboutStringKeys.Close, CultureInfo.GetCultureInfo("zh-CN"), out string zhClose).Should().BeTrue();
        feature.TryGet(AboutStringKeys.Close, CultureInfo.GetCultureInfo("en-US"), out string enClose).Should().BeTrue();
        feature.TryGet(AboutStringKeys.Copyright, CultureInfo.GetCultureInfo("zh-CN"), out string copyright).Should().BeTrue();

        // Assert
        zhClose.Should().Be("关闭");
        enClose.Should().Be("Close");
        copyright.Should().Be("© 2022-2026 稀布老咖。保留所有权利。");
    }

#nullable disable
    private sealed class FakeIoC(ILocalizationService localization, IMenuRegistry menus) : IIoCManager
    {
        public object Resolve(Type serviceType) =>
            serviceType == typeof(ILocalizationService) ? localization
            : serviceType == typeof(IMenuRegistry) ? menus
            : throw new InvalidOperationException($"测试容器未注册 {serviceType.Name}。");

        public T Resolve<T>() => (T)Resolve(typeof(T));

        public T ResolveKeyed<T>(object key) => default;
    }
#nullable restore
}
