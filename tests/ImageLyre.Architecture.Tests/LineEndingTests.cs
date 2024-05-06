using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace ImageLyre.Architecture.Tests;

/// <summary>校验仓库文本文件使用 LF 行尾，避免编辑器反复提示"行尾不一致"。</summary>
/// <remarks>
///     <para>
///         ``.editorconfig`` 与 ``.gitattributes`` 都要求 LF；但脚本化改写容易混入 CRLF，
///         混入之后文件会同时含两种行尾，Visual Studio 每次打开都提示标准化。
///     </para>
///     <para>
///         排除 ``*.slnx``（``.gitattributes`` 显式要求 CRLF）与 ``packages.lock.json``（由还原过程生成）。
///     </para>
/// </remarks>
public sealed class LineEndingTests
{
    private static readonly HashSet<string> s_checkedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".cs",
        ".csproj",
        ".props",
        ".targets",
        ".md",
        ".ps1",
        ".axaml",
        ".json",
    };

    [Fact]
    public void TextFilesUseLfOnly()
    {
        var offenders = new List<string>();

        foreach (var file in EnumerateTextFiles())
        {
            var bytes = File.ReadAllBytes(file);
            for (var index = 0; index < bytes.Length - 1; index++)
            {
                if (bytes[index] == '\r' && bytes[index + 1] == '\n')
                {
                    offenders.Add(Path.GetRelativePath(Repository.Root, file));
                    break;
                }
            }
        }

        Assert.True(
            offenders.Count == 0,
            "以下文件含 CRLF 行尾，编辑器会提示行尾不一致：" + Environment.NewLine + string.Join(Environment.NewLine, offenders));
    }

    private static IEnumerable<string> EnumerateTextFiles()
    {
        var skipDirectories = new[] { "bin", "obj", ".git", ".vs" };

        foreach (var directory in new[] { "src", "tests", "docs" })
        {
            var root = Path.Combine(Repository.Root, directory);
            if (!Directory.Exists(root))
            {
                continue;
            }

            foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
            {
                if (skipDirectories.Any(name => file.Contains($"{Path.DirectorySeparatorChar}{name}{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                if (!s_checkedExtensions.Contains(Path.GetExtension(file)))
                {
                    continue;
                }

                if (file.EndsWith(".lock.json", StringComparison.OrdinalIgnoreCase)
                    || file.EndsWith(".slnx", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                yield return file;
            }
        }

        foreach (var name in new[] { "build.ps1", "Directory.Build.props" })
        {
            var path = Path.Combine(Repository.Root, name);
            if (File.Exists(path))
            {
                yield return path;
            }
        }
    }
}
