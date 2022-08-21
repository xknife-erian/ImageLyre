using FluentAssertions;
using ImageLyre.Services.Importer;
using Xunit;

namespace UTest.ServicesTest.Importer;

public class ImportTests
{
    [Fact()]
    public void BaseTest()
    {
        var result = Import.Calc(11, 22, 33, 44);
        result.Should().Be((11 + 22) * (33 + 44));
    }
}