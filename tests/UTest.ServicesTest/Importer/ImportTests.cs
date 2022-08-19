using FluentAssertions;
using ImageLyre.Services.Importer;
using Xunit;

namespace UTest.ServicesTest.Importer;

public class ImportTests
{
    [Fact()]
    public void BaseTest()
    {
        var result = Import.Calc(1, 2, 3, 4);
        result.Should().Be((1 + 2) * (3 + 4));
    }
}