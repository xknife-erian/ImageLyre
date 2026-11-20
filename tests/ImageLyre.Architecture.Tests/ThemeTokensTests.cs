using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;

namespace ImageLyre.Architecture.Tests;

/// <summary>主题令牌的契约断言：两套字典键名同构、无重复、前缀统一，且被引用的键都存在。</summary>
public sealed class ThemeTokensTests
{
    private static readonly XNamespace s_xaml = "http://schemas.microsoft.com/winfx/2006/xaml";
    private static readonly Regex s_axamlReference = new(@"\{DynamicResource\s+(Lyre\.[A-Za-z0-9_.]+)\}", RegexOptions.Compiled);
    private static readonly Regex s_codeReference = new(@"DynamicResourceExtension\(\s*""(Lyre\.[A-Za-z0-9_.]+)""", RegexOptions.Compiled);

    [Fact]
    public void 亮暗字典都存在且非空()
    {
        var light = KeysOf("Tokens.Light.axaml");
        var dark = KeysOf("Tokens.Dark.axaml");

        Assert.NotEmpty(light);
        Assert.NotEmpty(dark);
    }

    [Fact]
    public void 亮暗两套字典键集合相等()
    {
        var light = KeysOf("Tokens.Light.axaml");
        var dark = KeysOf("Tokens.Dark.axaml");

        Assert.Equal(light.OrderBy(k => k, StringComparer.Ordinal), dark.OrderBy(k => k, StringComparer.Ordinal));
    }

    [Fact]
    public void 同一字典内键不重复()
    {
        foreach (var file in new[] { "Tokens.Light.axaml", "Tokens.Dark.axaml", "Tokens.Shared.axaml" })
        {
            var duplicated = AllKeyAttributes(file)
                .GroupBy(k => k, StringComparer.Ordinal)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToArray();

            Assert.True(duplicated.Length == 0, $"{file} 内出现重复键：{string.Join(", ", duplicated)}");
        }
    }

    [Fact]
    public void 所有键以Lyre前缀开头()
    {
        foreach (var file in new[] { "Tokens.Light.axaml", "Tokens.Dark.axaml", "Tokens.Shared.axaml" })
        {
            var offenders = KeysOf(file).Where(k => !k.StartsWith("Lyre.", StringComparison.Ordinal)).ToArray();

            Assert.True(offenders.Length == 0, $"{file} 内出现非 Lyre 前缀的键：{string.Join(", ", offenders)}");
        }
    }

    [Fact]
    public void 被引用的令牌键都存在()
    {
        var declared = new HashSet<string>(KeysOf("Tokens.Light.axaml"), StringComparer.Ordinal);
        declared.UnionWith(KeysOf("Tokens.Shared.axaml"));

        var references = new List<(string File, string Key)>();
        foreach (var path in SourceFiles())
        {
            var text = File.ReadAllText(path);
            var regex = path.EndsWith(".axaml", StringComparison.OrdinalIgnoreCase) ? s_axamlReference : s_codeReference;
            foreach (Match match in regex.Matches(text))
            {
                references.Add((Path.GetFileName(path), match.Groups[1].Value));
            }
        }

        Assert.True(references.Count > 0, "没有任何地方引用 Lyre.* 令牌：断言在空转");

        var missing = references.Where(r => !declared.Contains(r.Key)).ToArray();
        Assert.True(missing.Length == 0, "引用了未声明的令牌：" + string.Join(", ", missing.Select(m => $"{m.Key}（{m.File}）")));
    }

    private static string TokensDirectory => Path.Combine(Repository.Root, "src", "Libs", "ImageLyre.Themes");

    private static string[] KeysOf(string fileName)
    {
        var path = Path.Combine(TokensDirectory, fileName);
        Assert.True(File.Exists(path), $"缺少令牌字典：{fileName}");

        return XDocument.Load(path).Descendants()
            .Select(e => (string?)e.Attribute(s_xaml + "Key"))
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .Select(k => k!)
            .ToArray();
    }

    private static string[] AllKeyAttributes(string fileName) => KeysOf(fileName);

    private static IEnumerable<string> SourceFiles()
    {
        var source = Path.Combine(Repository.Root, "src");
        return Directory.EnumerateFiles(source, "*.axaml", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(source, "*.cs", SearchOption.AllDirectories))
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase));
    }
}
