using System.Reactive.Linq;
using FluentAssertions;
using ImageLyre.Contracts.Platform;
using ImageLyre.Module.Platform.Internal;
using Xunit;

namespace ImageLyre.Module.Platform.Tests;

/// <summary>守住平台能力矩阵与永不触发的变更流契约。</summary>
public sealed class PlatformCapabilitiesTests
{
    [Fact(DisplayName = "1.Windows 的八项平台能力全部返回 true")]
    public void Test001()
    {
        // Arrange
        var capabilities = new PlatformCapabilities(OperatingSystemKind.Windows);
        PlatformCapability[] all = Enum.GetValues<PlatformCapability>();

        // Act & Assert
        capabilities.CurrentOS.Should().Be(OperatingSystemKind.Windows);
        all.Should().HaveCount(8);
        all.Should().OnlyContain(capability => capabilities.Supports(capability));
    }

    [Theory(DisplayName = "2.macOS 与 Linux 的八项平台能力全部返回 false")]
    [InlineData(OperatingSystemKind.MacOS)]
    [InlineData(OperatingSystemKind.Linux)]
    public void Test002(OperatingSystemKind operatingSystem)
    {
        // Arrange
        var capabilities = new PlatformCapabilities(operatingSystem);

        // Act & Assert
        capabilities.CurrentOS.Should().Be(operatingSystem);
        Enum.GetValues<PlatformCapability>()
            .Should()
            .OnlyContain(capability => !capabilities.Supports(capability));
    }

    [Fact(DisplayName = "3.Unknown 平台的八项能力全部返回 false")]
    public void Test003()
    {
        // Arrange
        var capabilities = new PlatformCapabilities(OperatingSystemKind.Unknown);

        // Act & Assert
        capabilities.CurrentOS.Should().Be(OperatingSystemKind.Unknown);
        Enum.GetValues<PlatformCapability>()
            .Should()
            .OnlyContain(capability => !capabilities.Supports(capability));
    }

    [Fact(DisplayName = "4.CapabilityChanged 订阅后不产生任何元素")]
    public void Test004()
    {
        // Arrange
        var capabilities = new PlatformCapabilities(OperatingSystemKind.Windows);
        var observed = new List<PlatformCapability>();

        // Act
        using var subscription = capabilities.CapabilityChanged.Subscribe(observed.Add);

        // Assert
        observed.Should().BeEmpty();
    }
}
