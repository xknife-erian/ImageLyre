using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Cangming.PicoPlugins;
using Cangming.PicoPlugins.Core.Modules;
using ImageLyre.UI.Contracts.Commands;

namespace ImageLyre.Module.Commands;

/// <summary>命令注册中心：命令的唯一登记处，菜单、命令面板与快捷键都从这里读。</summary>
/// <remarks>
///     <para>
///         注册重复的命令标识直接抛异常而不是覆盖，因为静默覆盖会让两个功能抢同一个名字而无人察觉；
///         注销幂等，重复注销不报错。可用性只由 <see cref="CommandDescriptor.CanExecutePredicate" /> 决定，
///         不引入字符串表达式求值器。
///     </para>
///     <para>
///         线程模型：注册与注销视为单线程；<see cref="AvailableCommandsChanged" /> 在引发变化的那条线程上推送，
///         订阅方自行切回界面线程。
///     </para>
/// </remarks>
[Plugin(
    id: "ImageLyre.Commands.Module",
    name: "命令",
    alias: "Commands",
    description: "命令注册、可用性判定与执行")]
public sealed class CommandsModule : BasePicoModule<ICommandRegistry>, ICommandRegistry
{
    private readonly Dictionary<string, CommandDescriptor> _commands = new(StringComparer.Ordinal);
    private readonly Subject<Unit> _changed = new();

    /// <summary>创建命令模块。</summary>
    public CommandsModule()
    {
    }

    /// <inheritdoc />
    public IObservable<Unit> AvailableCommandsChanged => _changed.AsObservable();

    /// <inheritdoc />
    protected override ICommandRegistry Module => this;

    /// <inheritdoc />
    public IDisposable RegisterCommand(CommandDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        if (_commands.ContainsKey(descriptor.CommandId))
        {
            throw new InvalidOperationException($"命令 {descriptor.CommandId} 已注册，不允许静默覆盖。");
        }

        _commands[descriptor.CommandId] = descriptor;
        _changed.OnNext(Unit.Default);

        return new Registration(this, descriptor.CommandId);
    }

    /// <inheritdoc />
    public void UnregisterCommand(string commandId)
    {
        if (!string.IsNullOrWhiteSpace(commandId) && _commands.Remove(commandId))
        {
            _changed.OnNext(Unit.Default);
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<CommandDescriptor> GetAvailableCommands() =>
        _commands.Values.Where(d => d.CanExecutePredicate?.Invoke(null) ?? true).ToArray();

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(string commandId, object? parameter = null, CancellationToken cancellationToken = default)
    {
        if (!_commands.TryGetValue(commandId, out var descriptor))
        {
            throw new ArgumentException($"命令 {commandId} 不存在。", nameof(commandId));
        }

        if (!CanExecute(commandId, parameter))
        {
            throw new InvalidOperationException($"命令 {commandId} 在当前状态下不可执行。");
        }

        return await descriptor.ExecuteAsync(parameter, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public bool CanExecute(string commandId, object? parameter = null) =>
        _commands.TryGetValue(commandId, out var descriptor)
        && (descriptor.CanExecutePredicate?.Invoke(parameter) ?? true);

    /// <inheritdoc />
    public CommandDescriptor? GetCommand(string commandId) =>
        !string.IsNullOrWhiteSpace(commandId) && _commands.TryGetValue(commandId, out var descriptor)
            ? descriptor
            : null;

    private sealed class Registration(CommandsModule owner, string commandId) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            owner.UnregisterCommand(commandId);
        }
    }
}
