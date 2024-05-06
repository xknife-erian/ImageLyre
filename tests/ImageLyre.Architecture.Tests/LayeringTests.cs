using System;
using System.Linq;
using Xunit;

namespace ImageLyre.Architecture.Tests;

/// <summary>校验工程落点与依赖方向符合 DESIGN-0002 的基座约定。</summary>
public sealed class LayeringTests
{
    [Fact]
    public void EveryProjectSitsInOneOfTheRoleDirectories()
    {
        foreach (var project in Repository.Projects("src", "tests"))
        {
            Assert.True(
                project.Role is not null,
                $"工程 {project.Name} 不在约定位置（src/Apps、src/Libs、src/Modules、src/Features、tests），基座不接纳第六种落点。");
        }
    }

    [Fact]
    public void TestProjectsFollowTheSuffixConvention()
    {
        foreach (var project in Repository.Projects("tests"))
        {
            Assert.EndsWith(".Tests", project.Name, StringComparison.Ordinal);
            Assert.Equal(ProjectRoles.Test, project.Role);
        }
    }

    [Fact]
    public void LibrariesDoNotReferenceApplications()
    {
        var applications = Repository.Projects("src")
            .Where(project => project.Role == ProjectRoles.App)
            .Select(project => project.Name)
            .ToArray();

        foreach (var library in Repository.Projects("src").Where(project => project.Role == ProjectRoles.Lib))
        {
            foreach (var reference in library.ProjectReferences)
            {
                Assert.False(
                    applications.Contains(reference, StringComparer.Ordinal),
                    $"类库 {library.Name} 引用了应用入口 {reference}，依赖方向只能由应用指向类库。");
            }
        }
    }

    [Fact]
    public void ExecutableApplicationsDoNotReferenceEachOther()
    {
        var executables = Repository.Projects("src")
            .Where(project => project.Role == ProjectRoles.App && project.IsExecutable)
            .ToArray();

        foreach (var application in executables)
        {
            foreach (var reference in application.ProjectReferences)
            {
                Assert.False(
                    executables.Any(candidate => candidate.Name.Equals(reference, StringComparison.Ordinal)),
                    $"可执行入口 {application.Name} 引用了另一个可执行入口 {reference}，共用能力应下沉到类库型装配工程。");
            }
        }
    }
    [Fact]
    public void ContractsDoNotReferenceUiPackages()
    {
        string[] uiPrefixes = ["Avalonia", "AtomUI", "SkiaSharp"];

        var contracts = Repository.Projects("src").Single(project => project.Name == "ImageLyre.Contracts");

        var offenders = contracts.PackageReferences
            .Where(package => uiPrefixes.Any(prefix => package.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            .Concat(contracts.ProjectReferences.Where(reference =>
                reference.Contains("ImageLyre.UI.Contracts", StringComparison.Ordinal)
                || reference.Contains("ImageLyre.Plugins", StringComparison.Ordinal)))
            .ToArray();

        Assert.True(
            offenders.Length == 0,
            $"契约工程不得依赖界面栈或插件 SDK：{string.Join("、", offenders)}");
    }
}
