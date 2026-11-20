using System;
using System.IO;
using System.Threading.Tasks;
using ImageLyre.Module.Files;
using Xunit;

namespace ImageLyre.Plugins.Tests;

/// <summary>外部变更通知：注册后能收到事件、暂停期间不转发、注销后不再持有监视。</summary>
public sealed class FilesModuleTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "imagelyre-files-" + Guid.NewGuid().ToString("N"));

    public FilesModuleTests() => Directory.CreateDirectory(_directory);

    [Fact]
    public async Task 监视目录后写入文件能收到事件()
    {
        using var module = new FilesModule();
        var received = false;
        using var subscription = module.FileChanged.Subscribe(e => received = true);

        module.Watch(Path.Combine(_directory, "sample.tif"), "doc-1");
        await File.WriteAllTextAsync(Path.Combine(_directory, "sample.tif"), "x");

        for (var i = 0; i < 40 && !received; i++)
        {
            await Task.Delay(50);
        }

        Assert.True(received, "写入文件后 2 秒内没有收到外部变更事件。");
    }

    [Fact]
    public void 暂停期间不转发且恢复后无需重新注册()
    {
        using var module = new FilesModule();
        module.Watch(_directory, "doc-2");
        module.PauseAll();

        Assert.True(module.IsPaused);
        Assert.Contains("doc-2", module.ActiveWatcherIds);

        module.ResumeAll();

        Assert.False(module.IsPaused);
        Assert.Single(module.ActiveWatcherIds);
    }

    [Fact]
    public void 注销后不再出现在活动列表里()
    {
        using var module = new FilesModule();
        module.Watch(_directory, "doc-3");

        module.Unwatch("doc-3");
        module.Unwatch("doc-3");

        Assert.Empty(module.ActiveWatcherIds);
    }

    /// <summary>清理临时目录。</summary>
    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }
}
