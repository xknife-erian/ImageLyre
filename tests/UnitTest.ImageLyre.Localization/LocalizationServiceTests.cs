using System.Globalization;
using FluentAssertions;
using ImageLyre.Contracts;
using ImageLyre.Localization;
using Xunit;

namespace ImageLyre.Localization.Tests;

/// <summary>守住 <see cref="LocalizationService"/> 的缺键回退、来源隔离、切换广播与注册幂等四条契约。</summary>
public sealed class LocalizationServiceTests
{
    #region 缺键与回退

    [Fact(DisplayName = "1.没有任何资源源时取键返回键名并记录一条缺翻译诊断")]
    public void Test001()
    {
        // Arrange
        var diagnostics = new List<string>();
        var service = new LocalizationService(diagnostics.Add);

        // Act
        var text = service.Get("lyre.shell.menu.file");

        // Assert
        text.Should().Be("lyre.shell.menu.file");
        diagnostics.Should().ContainSingle().Which.Should().Contain("lyre.shell.menu.file");
    }

    #endregion

    #region 来源注册与解析

    [Fact(DisplayName = "2.已注册的来源能解析它自己命名空间的键")]
    public void Test101()
    {
        // Arrange
        var service = new LocalizationService();
        service.AddSource(new FakeSource(("lyre.shell.menu.file", "zh-CN", "文件")));

        // Act
        var text = service.Get("lyre.shell.menu.file");

        // Assert
        text.Should().Be("文件");
    }

    [Fact(DisplayName = "3.重复注册同一来源时只问一次，缺键诊断也不重复")]
    public void Test102()
    {
        // Arrange
        var diagnostics = new List<string>();
        var source = new FakeSource();
        var service = new LocalizationService(diagnostics.Add);
        service.AddSource(source);
        service.AddSource(source);

        // Act
        var text = service.Get("lyre.shell.menu.missing");

        // Assert
        text.Should().Be("lyre.shell.menu.missing");
        source.CallCount.Should().Be(1);
        diagnostics.Should().ContainSingle();
    }

    #endregion

    #region 语言切换与回退

    [Fact(DisplayName = "4.切换语言后 Current 更新、只广播一次，随后按新语言取文案")]
    public async Task Test201()
    {
        // Arrange
        var service = new LocalizationService();
        var raised = 0;
        service.LanguageChanged += (_, _) => raised++;
        service.AddSource(new FakeSource(
            ("lyre.shell.menu.file", "zh-CN", "文件"),
            ("lyre.shell.menu.file", "en-US", "File")));

        // Act
        await service.SetLanguageAsync("en-US");

        // Assert
        service.Current.Name.Should().Be("en-US");
        raised.Should().Be(1);
        service.Get("lyre.shell.menu.file").Should().Be("File");
    }

    [Fact(DisplayName = "5.当前语言缺键时回退到 zh-CN 的文案")]
    public async Task Test202()
    {
        // Arrange
        var service = new LocalizationService();
        service.AddSource(new FakeSource(("lyre.shell.menu.help", "zh-CN", "帮助")));
        await service.SetLanguageAsync("en-US");

        // Act
        var text = service.Get("lyre.shell.menu.help");

        // Assert
        text.Should().Be("帮助");
    }

    [Fact(DisplayName = "6.语言名无法识别时抛 CultureNotFoundException，当前语言与事件都不变")]
    public async Task Test203()
    {
        // Arrange
        var service = new LocalizationService();
        var raised = 0;
        service.LanguageChanged += (_, _) => raised++;

        // Act
        var act = async () => await service.SetLanguageAsync("not-a-culture-name");

        // Assert
        await act.Should().ThrowAsync<CultureNotFoundException>();
        service.Current.Name.Should().Be("zh-CN");
        raised.Should().Be(0);
    }

    #endregion

    #region 故障隔离

    [Fact(DisplayName = "7.资源源抛异常时被隔离并记诊断，其余来源照常解析")]
    public void Test204()
    {
        // Arrange
        var diagnostics = new List<string>();
        var service = new LocalizationService(diagnostics.Add);
        service.AddSource(new ThrowingSource());
        service.AddSource(new FakeSource(("lyre.shell.menu.file", "zh-CN", "文件")));

        // Act
        var text = service.Get("lyre.shell.menu.file");

        // Assert
        text.Should().Be("文件");
        diagnostics.Should().ContainSingle().Which.Should().Contain(nameof(ThrowingSource));
    }

    #endregion

    private sealed class ThrowingSource : ILocalizationSource
    {
        public bool TryGet(string key, CultureInfo culture, out string value)
            => throw new InvalidOperationException("来源故意抛异常");
    }

    private sealed class FakeSource : ILocalizationSource
    {
        private readonly Dictionary<(string Key, string Culture), string> _entries;

        public FakeSource(params (string Key, string Culture, string Text)[] entries)
            => _entries = entries.ToDictionary(entry => (entry.Key, entry.Culture), entry => entry.Text);

        public int CallCount { get; private set; }

        public bool TryGet(string key, CultureInfo culture, out string value)
        {
            CallCount++;

            if (_entries.TryGetValue((key, culture.Name), out var text))
            {
                value = text;
                return true;
            }

            value = string.Empty;
            return false;
        }
    }
}
