using FluentAssertions;
using ImageLad.Services.Importer;
using Xunit;

namespace UTest.ServicesTest.Importer
{
    public class ImportTests
    {
        [Fact()]
        public void BaseTest()
        {
            var result = Import.sum(1, 2);
            result.Should().Be(3);
        }
    }
}