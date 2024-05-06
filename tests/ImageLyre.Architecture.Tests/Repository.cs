using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ImageLyre.Architecture.Tests;

/// <summary>提供从测试程序集定位仓库根目录、读取工程与读取产品级属性的能力。</summary>
internal static class Repository
{
    private static readonly Lazy<string> s_root = new(FindRoot);
    private static readonly Lazy<string> s_productName = new(ReadProductName);

    /// <summary>获取仓库根目录的绝对路径。</summary>
    internal static string Root => s_root.Value;

    /// <summary>获取 <c>Directory.Build.props</c> 中声明的产品名，用作命名空间前缀。</summary>
    internal static string ProductName => s_productName.Value;

    /// <summary>枚举指定相对目录下的全部工程文件。</summary>
    /// <param name="relativeDirectories">相对仓库根目录的目录列表，例如 <c>src</c>。</param>
    /// <returns>按路径排序的工程文件集合。</returns>
    internal static IEnumerable<ProjectFile> Projects(params string[] relativeDirectories)
    {
        var projects = new List<ProjectFile>();
        foreach (var relative in relativeDirectories)
        {
            var directory = Path.Combine(Root, relative.Replace('/', Path.DirectorySeparatorChar));
            if (!Directory.Exists(directory))
            {
                continue;
            }

            projects.AddRange(Directory.EnumerateFiles(directory, "*.csproj", SearchOption.AllDirectories)
                .OrderBy(path => path, StringComparer.Ordinal)
                .Select(ProjectFile.Load));
        }

        return projects;
    }

    /// <summary>按路径推断工程角色：<c>src/Apps</c> 为 App，<c>src/Libs</c> 为 Lib，<c>tests</c> 为 Test，其余为空。</summary>
    /// <param name="path">工程文件的绝对路径。</param>
    /// <returns>角色名；不在三类约定位置时返回 <see langword="null"/>。</returns>
    internal static string? RoleOf(string path)
    {
        var normalized = Path.GetFullPath(path).Replace('\\', '/');
        var root = Root.Replace('\\', '/');
        if (normalized.StartsWith(root + "/src/Apps/", StringComparison.OrdinalIgnoreCase))
        {
            return ProjectRoles.App;
        }

        if (normalized.StartsWith(root + "/src/Libs/", StringComparison.OrdinalIgnoreCase))
        {
            return ProjectRoles.Lib;
        }

        if (normalized.StartsWith(root + "/src/Modules/", StringComparison.OrdinalIgnoreCase))
        {
            return ProjectRoles.Module;
        }

        if (normalized.StartsWith(root + "/src/Features/", StringComparison.OrdinalIgnoreCase))
        {
            return ProjectRoles.Feature;
        }

        if (normalized.StartsWith(root + "/tests/", StringComparison.OrdinalIgnoreCase))
        {
            return ProjectRoles.Test;
        }

        return null;
    }

    private static string FindRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "src", "ImageLyre.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("未找到仓库根目录：从测试程序集目录向上未发现 src/ImageLyre.slnx。");
    }

    private static string ReadProductName()
    {
        var props = Path.Combine(Root, "Directory.Build.props");
        var product = XDocument.Load(props)
            .Descendants("Product")
            .Select(element => element.Value.Trim())
            .FirstOrDefault(value => value.Length > 0);
        return product ?? Path.GetFileNameWithoutExtension(Root);
    }
}

/// <summary>工程角色的固定取值。</summary>
internal static class ProjectRoles
{
    /// <summary>应用入口与装配层，位于 <c>src/Apps</c>。</summary>
    internal const string App = "App";

    /// <summary>类库，位于 <c>src/Libs</c>。</summary>
    internal const string Lib = "Lib";

    /// <summary>插件容器模块，位于 <c>src/Modules</c>。</summary>
    internal const string Module = "Module";

    /// <summary>界面功能插件，位于 <c>src/Features</c>。</summary>
    internal const string Feature = "Feature";

    /// <summary>测试工程，位于 <c>tests</c>。</summary>
    internal const string Test = "Test";
}

/// <summary>描述一个工程文件的名称、角色、输出类型、工程引用与包引用。</summary>
internal sealed class ProjectFile
{
    private ProjectFile(
        string name,
        string fullPath,
        string? role,
        string outputType,
        IReadOnlyList<string> projectReferences,
        IReadOnlyList<string> packageReferences)
    {
        Name = name;
        FullPath = fullPath;
        Role = role;
        OutputType = outputType;
        ProjectReferences = projectReferences;
        PackageReferences = packageReferences;
    }

    /// <summary>获取不含扩展名的工程名。</summary>
    internal string Name { get; }

    /// <summary>获取工程文件的绝对路径。</summary>
    internal string FullPath { get; }

    /// <summary>获取工程角色，取值见 <see cref="ProjectRoles"/>。</summary>
    internal string? Role { get; }

    /// <summary>获取工程声明的输出类型；未声明时为空字符串。</summary>
    internal string OutputType { get; }

    /// <summary>获取工程是否产出可执行文件。</summary>
    internal bool IsExecutable =>
        OutputType.Equals("Exe", StringComparison.OrdinalIgnoreCase)
        || OutputType.Equals("WinExe", StringComparison.OrdinalIgnoreCase);

    /// <summary>获取被引用工程的名称集合。</summary>
    internal IReadOnlyList<string> ProjectReferences { get; }

    /// <summary>获取直接引用的包标识集合。</summary>
    internal IReadOnlyList<string> PackageReferences { get; }

    /// <summary>读取并解析一个工程文件。</summary>
    /// <param name="path">工程文件的绝对路径。</param>
    /// <returns>解析后的工程信息。</returns>
    internal static ProjectFile Load(string path)
    {
        var document = XDocument.Load(path);
        var projectReferences = document.Descendants("ProjectReference")
            .Select(element => Path.GetFileNameWithoutExtension((string?)element.Attribute("Include") ?? string.Empty))
            .Where(name => name.Length > 0)
            .ToArray();
        var packageReferences = document.Descendants("PackageReference")
            .Select(element => (string?)element.Attribute("Include") ?? string.Empty)
            .Where(name => name.Length > 0)
            .ToArray();
        var outputType = document.Descendants("OutputType").FirstOrDefault()?.Value.Trim() ?? string.Empty;
        return new ProjectFile(
            Path.GetFileNameWithoutExtension(path),
            path,
            Repository.RoleOf(path),
            outputType,
            projectReferences,
            packageReferences);
    }
}
