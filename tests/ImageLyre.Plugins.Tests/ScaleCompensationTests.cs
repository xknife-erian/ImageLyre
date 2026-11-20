using ImageLyre.Module.Workbench.Scaling;
using Xunit;

namespace ImageLyre.Plugins.Tests;

/// <summary>缩放补偿：任何倍率组合下，发丝线与信号轨的物理像素都保持不变。</summary>
public sealed class ScaleCompensationTests
{
    [Theory]
    [InlineData(1.0, 1.0)]
    [InlineData(1.25, 1.0)]
    [InlineData(1.5, 1.5)]
    [InlineData(2.0, 2.0)]
    [InlineData(1.25, 2.0)]
    public void 发丝线在任何倍率下都是一物理像素(double renderScaling, double appZoom)
    {
        var dips = ScaleCompensation.Hairline(renderScaling, appZoom);

        Assert.Equal(1.0, dips * renderScaling * appZoom, precision: 9);
    }

    [Theory]
    [InlineData(1.0, 1.5)]
    [InlineData(1.5, 2.0)]
    public void 信号轨保持两物理像素(double renderScaling, double appZoom)
    {
        var dips = ScaleCompensation.Compensate(2, renderScaling, appZoom);

        Assert.Equal(2.0, dips * renderScaling * appZoom, precision: 9);
    }

    [Fact]
    public void 倍率非法时回落到基准值而不是崩溃()
    {
        Assert.Equal(1, ScaleCompensation.Hairline(0, 1));
        Assert.Equal(2, ScaleCompensation.Compensate(2, double.NaN, 1));
    }
}
