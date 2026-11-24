using ImageLyre.Bootstrap.Startup;

namespace ImageLyre.Presentation.Desktop.Startup;

/// <summary>Presentation 阶段的固定 Plan：按 welcome、composition、show 三组组织桌面界面步骤。</summary>
/// <remarks>
/// <para>
/// 组间顺序由 <see cref="Create"/> 的构造参数写死，组内顺序由组自身的 Step 列表决定；<c>welcome</c> 组当前为空，
/// 因为欢迎窗必须在 Host 启动前出现才能覆盖插件启动期，它由 <see cref="DesktopStartup"/> 直接创建，不进入 Plan。
/// 空组仍参与报告分组与顺序断言，不用空操作步骤凑容量。
/// </para>
/// <para>
/// 线程：本 Plan 只在 Avalonia UI 线程上执行，步骤在构造期不碰控件，执行期才创建窗口与槽位。
/// </para>
/// </remarks>
internal static class PresentationPlan
{
    /// <summary>创建 Presentation 阶段的固定 Plan。</summary>
    /// <returns>每次调用返回一份新的、组序固定的 Plan；其中的步骤无状态。</returns>
    internal static BootstrapPlan<DesktopBootstrapContext> Create() =>
        new(
        [
            new BootstrapGroup<DesktopBootstrapContext>("welcome", []),
            new BootstrapGroup<DesktopBootstrapContext>(
                "composition",
                [
                    new CreateSlotManagerStep(),
                    new InjectSlotManagerStep(),
                    new BuildShellStep(),
                    new AttachFeaturesStep(),
                ]),
            new BootstrapGroup<DesktopBootstrapContext>(
                "show",
                [
                    new ShowMainWindowStep(),
                    new CloseWelcomeStep(),
                ]),
        ]);
}
