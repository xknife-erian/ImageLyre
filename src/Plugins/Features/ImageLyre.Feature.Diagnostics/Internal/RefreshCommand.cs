using System.Windows.Input;

namespace ImageLyre.Feature.Diagnostics.Internal;

/// <summary>诊断面板的最小刷新命令：始终可执行，执行时调用注入的刷新动作。</summary>
/// <remarks>
/// M1 不需要运行期禁用或参数，因此 <see cref="CanExecuteChanged"/> 不提供订阅点；
/// 后续若需要按可用条件灰显，先换成带 <see cref="EventHandler"/> 的实现，再在工具条上订阅。
/// </remarks>
internal sealed class RefreshCommand : ICommand
{
    private readonly Action _execute;

    /// <summary>创建一个刷新命令。</summary>
    /// <param name="execute">点击时执行的动作，不可为 <see langword="null"/>。</param>
    internal RefreshCommand(Action execute)
    {
        ArgumentNullException.ThrowIfNull(execute);
        _execute = execute;
    }

    /// <summary>可用状态变化事件；M1 没有可用状态变化，订阅与退订均为空操作。</summary>
    public event EventHandler? CanExecuteChanged
    {
        add { }
        remove { }
    }

    /// <summary>刷新命令始终可执行。</summary>
    /// <param name="parameter">命令参数，本实现不使用。</param>
    /// <returns>恒为 <see langword="true"/>。</returns>
    public bool CanExecute(object? parameter) => true;

    /// <summary>执行一次面板刷新。</summary>
    /// <param name="parameter">命令参数，本实现不使用。</param>
    public void Execute(object? parameter) => _execute();
}
