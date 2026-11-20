using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageLyre.Module.Preferences;
using Xunit;

namespace ImageLyre.Plugins.Tests;

/// <summary>偏好与习惯：读写、变更通知、落盘与重载。</summary>
public sealed class PreferencesModuleTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "imagelyre-prefs-" + Guid.NewGuid().ToString("N"));

    [Fact]
    public void 写入后可按类型读回()
    {
        var module = new PreferencesModule();
        module.UseStorageRoot(_root);

        module.SetValue("Editor.FontSize", 13);
        module.SetValue("Editor.Theme", "Dark");

        Assert.Equal(13, module.GetValue("Editor.FontSize", 0));
        Assert.Equal("Dark", module.GetValue("Editor.Theme", "Light"));
        Assert.True(module.Contains("Editor.FontSize"));
        Assert.Equal(0, module.GetValue("Missing.Key", 0));
    }

    [Fact]
    public void 变更通知带旧值与新值()
    {
        var module = new PreferencesModule();
        module.UseStorageRoot(_root);
        var observed = Array.Empty<(string Key, object? OldValue, object? NewValue)>();
        using var subscription = module.PreferenceChanged.Subscribe(change => observed = [change]);

        module.SetValue("Editor.TabSize", 4);

        var change = Assert.Single(observed);
        Assert.Equal("Editor.TabSize", change.Key);
    }

    [Fact]
    public async Task 落盘后新实例能读回偏好与习惯()
    {
        var writer = new PreferencesModule();
        writer.UseStorageRoot(_root);
        writer.SetValue("Window.Width", 1440);
        writer.UserState.RecordValue("Session.LastDocument", "sample.tif");
        await writer.FlushAsync();
        writer.UserState.Save();

        var reader = new PreferencesModule();
        reader.UseStorageRoot(_root);

        Assert.Equal(1440, reader.GetValue("Window.Width", 0));
        Assert.Equal("sample.tif", reader.UserState.GetValue("Session.LastDocument", string.Empty));
    }

    [Fact]
    public void 移除键会通知并让键消失()
    {
        var module = new PreferencesModule();
        module.UseStorageRoot(_root);
        module.SetValue("View.RailWidth", 44);

        module.Remove("View.RailWidth");

        Assert.False(module.Contains("View.RailWidth"));
        Assert.DoesNotContain(module.ExportAll(), p => p.Key == "View.RailWidth");
    }

    /// <summary>清理临时目录。</summary>
    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }
}
