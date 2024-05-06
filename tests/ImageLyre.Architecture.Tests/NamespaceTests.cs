using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace ImageLyre.Architecture.Tests;

/// <summary>校验源代码的命名空间与所属工程一致，且以产品名开头。</summary>
public sealed class NamespaceTests
{
    private static readonly Regex s_namespacePattern = new(
        @"^\s*namespace\s+(?<ns>[A-Za-z_][A-Za-z0-9_.]*)",
        RegexOptions.Multiline | RegexOptions.CultureInvariant);

    [Fact]
    public void SourceNamespacesMatchTheirProjects()
    {
        var prefix = Repository.ProductName + ".";
        foreach (var (file, projectName) in SourceFiles())
        {
            var match = s_namespacePattern.Match(File.ReadAllText(file));
            Assert.True(match.Success, $"{Relative(file)} 未声明命名空间。");
            var ns = match.Groups["ns"].Value;
            Assert.True(
                ns.StartsWith(prefix, StringComparison.Ordinal),
                $"{Relative(file)} 的命名空间 {ns} 未以 {prefix} 开头。");
            Assert.True(
                ns.Equals(projectName, StringComparison.Ordinal) || ns.StartsWith(projectName + ".", StringComparison.Ordinal),
                $"{Relative(file)} 的命名空间 {ns} 与工程 {projectName} 不对应。");
        }
    }

    private static IEnumerable<(string File, string ProjectName)> SourceFiles()
    {
        var sourceRoot = Path.Combine(Repository.Root, "src");
        if (!Directory.Exists(sourceRoot))
        {
            return [];
        }

        return Directory
            .EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !IsBuildOutput(path))
            .OrderBy(path => path, StringComparer.Ordinal)
            .Select(path => (path, ProjectNameOf(path)));
    }

    private static bool IsBuildOutput(string path)
        => path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
            || path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);

    private static string ProjectNameOf(string file)
    {
        var directory = new DirectoryInfo(Path.GetDirectoryName(file) ?? Repository.Root);
        while (directory is not null)
        {
            var project = Directory.EnumerateFiles(directory.FullName, "*.csproj").FirstOrDefault();
            if (project is not null)
            {
                return Path.GetFileNameWithoutExtension(project);
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException($"{file} 未找到所属工程。");
    }

    private static string Relative(string path) => Path.GetRelativePath(Repository.Root, path);
}
