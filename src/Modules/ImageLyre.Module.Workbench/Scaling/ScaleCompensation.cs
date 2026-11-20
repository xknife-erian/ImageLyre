using System;

namespace ImageLyre.Module.Workbench.Scaling;

/// <summary>缩放补偿：把发丝线换算成反比厚度，保证任何缩放倍率下都是 1 物理像素。</summary>
/// <remarks>
///     <para>
///         令牌里的 <c>Lyre.Size.Hairline</c> 是 1 DIP 的基准值，不随缩放重算；渲染时用本类把它除以
///         "系统 DPI 倍率 × 应用内缩放倍率"，于是物理像素恒为 1。
///     </para>
///     <para>
///         2px 信号轨同理，用 <see cref="Compensate(double, double, double)"/> 传 2 即可；两处都不进字典，
///         因为它们是渲染期换算而不是新的令牌值。
///     </para>
/// </remarks>
public static class ScaleCompensation
{
    /// <summary>把基准 DIP 厚度换算成补偿后的 DIP 厚度。</summary>
    /// <param name="baseDips">基准厚度，例如发丝线 1、信号轨 2。</param>
    /// <param name="renderScaling">系统 DPI 倍率，取 <c>TopLevel.RenderScaling</c>。</param>
    /// <param name="appZoom">应用内缩放倍率，1 表示 100%。</param>
    /// <returns>补偿后的 DIP 厚度；倍率非法时按 1 处理，避免除零。</returns>
    public static double Compensate(double baseDips, double renderScaling, double appZoom)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(baseDips);

        var factor = renderScaling * appZoom;
        if (double.IsNaN(factor) || double.IsInfinity(factor) || factor <= 0)
        {
            return baseDips;
        }

        return baseDips / factor;
    }

    /// <summary>发丝线的补偿厚度，等价于 <see cref="Compensate"/> 传 1。</summary>
    /// <param name="renderScaling">系统 DPI 倍率。</param>
    /// <param name="appZoom">应用内缩放倍率。</param>
    /// <returns>补偿后的 DIP 厚度。</returns>
    public static double Hairline(double renderScaling, double appZoom) => Compensate(1, renderScaling, appZoom);
}
