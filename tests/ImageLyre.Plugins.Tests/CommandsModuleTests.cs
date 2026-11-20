using System;
using System.Linq;
using System.Threading.Tasks;
using ImageLyre.Module.Commands;
using ImageLyre.UI.Contracts.Commands;
using Xunit;

namespace ImageLyre.Plugins.Tests;

/// <summary>命令注册中心的硬行为：重复注册拒绝、可用性由谓词决定、注销幂等。</summary>
public sealed class CommandsModuleTests
{
    [Fact]
    public void 重复注册同一命令会抛异常()
    {
        var module = new CommandsModule();
        module.RegisterCommand(Descriptor("Edit.Copy"));

        Assert.Throws<InvalidOperationException>(() => module.RegisterCommand(Descriptor("Edit.Copy")));
    }

    [Fact]
    public async Task 可用性由谓词决定并影响执行()
    {
        var module = new CommandsModule();
        var enabled = false;
        module.RegisterCommand(new CommandDescriptor
        {
            CommandId = "Edit.Paste",
            DisplayName = "粘贴",
            ExecuteAsync = (_, _) => Task.FromResult(CommandResult.Success()),
            CanExecutePredicate = _ => enabled,
        });

        Assert.False(module.CanExecute("Edit.Paste"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => module.ExecuteAsync("Edit.Paste"));

        enabled = true;

        Assert.True(module.CanExecute("Edit.Paste"));
        Assert.True((await module.ExecuteAsync("Edit.Paste")).IsSuccess);
    }

    [Fact]
    public void 注册令牌释放后命令消失且重复注销不报错()
    {
        var module = new CommandsModule();
        var token = module.RegisterCommand(Descriptor("View.Zoom"));

        Assert.NotNull(module.GetCommand("View.Zoom"));

        token.Dispose();
        token.Dispose();

        Assert.Null(module.GetCommand("View.Zoom"));
        Assert.DoesNotContain(module.GetAvailableCommands(), d => d.CommandId == "View.Zoom");
    }

    private static CommandDescriptor Descriptor(string id) => new()
    {
        CommandId = id,
        DisplayName = id,
        ExecuteAsync = (_, _) => Task.FromResult(CommandResult.Success()),
    };
}
