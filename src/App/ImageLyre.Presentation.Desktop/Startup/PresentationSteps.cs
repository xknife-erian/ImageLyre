using Avalonia.Controls;
using Cangming.PicoPlugins.Core.Features;
using ImageLyre.Bootstrap.Startup;

namespace ImageLyre.Presentation.Desktop.Startup;

/// <summary>Presentation 步骤的共同骨架：声明显示阶段与不可回滚，并把同步步骤的异常收敛成 <see cref="StepOutcome.Failed"/>。</summary>
/// <remarks>
/// <para>
/// 每个步骤只实现识别信息与 <see cref="Execute"/>；失败统一由本类映射为带原始异常的 <see cref="StepResult"/>，
/// 这样宿主在失败信息里能读到具体 Step Id，而不是只剩调用栈。回滚由 T09 的桌面 StopPlan 接手，本骨架一律声明不可回滚。
/// </para>
/// <para>
/// 步骤在构造期不碰控件，只在 <see cref="Execute"/> 里读写 Avalonia 对象，因此与 <see cref="StepPhase.Presentation"/> 的线程约束一致。
/// </para>
/// </remarks>
internal abstract class PresentationStep : IBootstrapStep<DesktopBootstrapContext>
{
    /// <inheritdoc />
    public abstract string Id { get; }

    /// <inheritdoc />
    public abstract string Group { get; }

    /// <inheritdoc />
    public StepPhase Phase => StepPhase.Presentation;

    /// <inheritdoc />
    public bool CanRollback => false;

    /// <inheritdoc />
    public ValueTask<StepResult> ExecuteAsync(DesktopBootstrapContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            Execute(context);
            return new ValueTask<StepResult>(new StepResult(StepOutcome.Completed));
        }
        catch (Exception ex)
        {
            return new ValueTask<StepResult>(new StepResult(StepOutcome.Failed, ex));
        }
    }

    /// <inheritdoc />
    public ValueTask RollbackAsync(DesktopBootstrapContext context, CancellationToken cancellationToken) =>
        ValueTask.CompletedTask;

    /// <summary>执行本步骤的界面动作；抛出异常即表示启动失败，由基类收敛为失败结果。</summary>
    /// <param name="context">Presentation 阶段上下文，提供桌面生命周期、欢迎窗与已装配对象。</param>
    protected abstract void Execute(DesktopBootstrapContext context);
}

/// <summary>装配组第一步：创建本次启动唯一的槽位管理器，后续注入、壳面板与 Feature 附着共用它。</summary>
internal sealed class CreateSlotManagerStep : PresentationStep
{
    /// <inheritdoc />
    public override string Id => "presentation.composition.slots";

    /// <inheritdoc />
    public override string Group => "composition";

    /// <inheritdoc />
    protected override void Execute(DesktopBootstrapContext context) => context.SlotManager = new SlotManager();

    // ponytail: 唯一性只由 Plan 的“先装配后显示”和我这一处 new 保证；重复创建由后续步骤的空值断言兜住。
}

/// <summary>装配组第二步：把槽位管理器注入插件管理器，壳在稍后取壳时才读取它。</summary>
internal sealed class InjectSlotManagerStep : PresentationStep
{
    /// <inheritdoc />
    public override string Id => "presentation.composition.inject";

    /// <inheritdoc />
    public override string Group => "composition";

    /// <inheritdoc />
    protected override void Execute(DesktopBootstrapContext context)
    {
        SlotManager slotManager = context.SlotManager
            ?? throw new InvalidOperationException("槽位管理器尚未创建，不能在注入步骤之前取壳或附着。");

        MainWindowFactory.InjectSlotManager(context.PluginManager, slotManager);
    }
}

/// <summary>装配组第三步：取壳并断言四个一级槽位已经贡献，把窗体引用交给显示组。</summary>
internal sealed class BuildShellStep : PresentationStep
{
    /// <inheritdoc />
    public override string Id => "presentation.composition.shell";

    /// <inheritdoc />
    public override string Group => "composition";

    /// <inheritdoc />
    protected override void Execute(DesktopBootstrapContext context)
    {
        SlotManager slotManager = context.SlotManager
            ?? throw new InvalidOperationException("槽位管理器尚未创建，不能取壳。");

        context.ShellWindow = MainWindowFactory.BuildShell(context.PluginManager, slotManager);
    }
}

/// <summary>装配组第四步：向已贡献槽位的壳附着全部 Feature 内容。</summary>
internal sealed class AttachFeaturesStep : PresentationStep
{
    /// <inheritdoc />
    public override string Id => "presentation.composition.attach";

    /// <inheritdoc />
    public override string Group => "composition";

    /// <inheritdoc />
    protected override void Execute(DesktopBootstrapContext context)
    {
        SlotManager slotManager = context.SlotManager
            ?? throw new InvalidOperationException("槽位管理器尚未创建，不能附着 Feature。");

        MainWindowFactory.AttachFeatures(context.PluginManager, slotManager);
    }
}

/// <summary>显示组第一步：把壳窗体换到桌面主窗口并显示。</summary>
internal sealed class ShowMainWindowStep : PresentationStep
{
    /// <inheritdoc />
    public override string Id => "presentation.show.main-window";

    /// <inheritdoc />
    public override string Group => "show";

    /// <inheritdoc />
    protected override void Execute(DesktopBootstrapContext context)
    {
        Window window = context.ShellWindow
            ?? throw new InvalidOperationException("壳窗体尚未取出，不能显示主窗体。");

        context.Desktop.MainWindow = window;
        window.Show();
    }
}

/// <summary>显示组第二步：主窗体显示后关闭欢迎窗口。</summary>
internal sealed class CloseWelcomeStep : PresentationStep
{
    /// <inheritdoc />
    public override string Id => "presentation.show.close-welcome";

    /// <inheritdoc />
    public override string Group => "show";

    /// <inheritdoc />
    protected override void Execute(DesktopBootstrapContext context) => context.Welcome.Close();
}
