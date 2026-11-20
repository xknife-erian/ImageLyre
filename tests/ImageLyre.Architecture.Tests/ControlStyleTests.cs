using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace ImageLyre.Architecture.Tests;

/// <summary>控件样式只允许引用四档字号与两条家族令牌，不得出现字号字面量。</summary>
public sealed class ControlStyleTests
{
    private static readonly Regex s_fontSize = new("Property=\"FontSize\"\\s+Value=\"([^\"]+)\"", RegexOptions.Compiled);
    private static readonly Regex s_literal = new("^\\d+(\\.\\d+)?$", RegexOptions.Compiled);

    [Fact]
    public void 控件样式的字号只引用令牌()
    {
        var path = Path.Combine(Repository.Root, "src", "Libs", "ImageLyre.Themes", "Controls.axaml");
        Assert.True(File.Exists(path), "缺少 Controls.axaml：A4 的控件样式入口不存在。");

        var text = File.ReadAllText(path);
        var values = s_fontSize.Matches(text).Select(m => m.Groups[1].Value).ToArray();

        Assert.NotEmpty(values);
        var literals = values.Where(v => s_literal.IsMatch(v)).ToArray();
        Assert.True(literals.Length == 0, "控件样式里出现字号字面量：" + string.Join(", ", literals));

        var outsiders = values
            .Where(v => !v.Contains("Lyre.Type.Size.", StringComparison.Ordinal))
            .ToArray();
        Assert.True(outsiders.Length == 0, "控件样式引用了非字号令牌：" + string.Join(", ", outsiders));
    }

    [Fact]
    public void 控件样式覆盖计划要求的六个控件()
    {
        var path = Path.Combine(Repository.Root, "src", "Libs", "ImageLyre.Themes", "Controls.axaml");
        var text = File.ReadAllText(path);

        foreach (var control in new[] { "Button", "TextBox", "ComboBox", "CheckBox", "MenuItem", "TextBlock" })
        {
            Assert.Contains($"Selector=\"{control}\"", text, StringComparison.Ordinal);
        }
    }
}
