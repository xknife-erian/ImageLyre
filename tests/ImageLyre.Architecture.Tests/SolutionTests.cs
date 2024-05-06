using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace ImageLyre.Architecture.Tests;

/// <summary>校验解决方案登记与磁盘上的工程文件一致，避免出现能构建但未被入口收录的工程。</summary>
public sealed class SolutionTests
{
    private static readonly string s_solutionPath = Path.Combine(Repository.Root, "src", "ImageLyre.slnx");

    [Fact]
    public void EverySourceAndTestProjectIsRegistered()
    {
        var registered = RegisteredProjects();
        var actual = AllProjects();
        var missing = actual.Where(path => !registered.Contains(path, StringComparer.OrdinalIgnoreCase)).ToArray();
        Assert.True(
            missing.Length == 0,
            $"以下工程未登记到 src/ImageLyre.slnx：{string.Join("、", missing.Select(Relative))}。");
    }

    [Fact]
    public void EveryRegisteredProjectExists()
    {
        var missing = RegisteredProjects()
            .Where(path => !File.Exists(path))
            .ToArray();
        Assert.True(
            missing.Length == 0,
            $"src/ImageLyre.slnx 登记的工程不存在：{string.Join("、", missing.Select(path => path))}。");
    }

    private static string[] RegisteredProjects()
    {
        var solutionDirectory = Path.GetDirectoryName(s_solutionPath) ?? Repository.Root;
        return XDocument.Load(s_solutionPath)
            .Descendants("Project")
            .Select(element => (string?)element.Attribute("Path"))
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => Path.GetFullPath(Path.Combine(solutionDirectory, path!.Replace('/', Path.DirectorySeparatorChar))))
            .ToArray();
    }

    private static string[] AllProjects()
        => Repository.Projects("src", "tests")
            .Select(project => Path.GetFullPath(project.FullPath))
            .ToArray();

    private static string Relative(string path) => Path.GetRelativePath(Repository.Root, path);
}
