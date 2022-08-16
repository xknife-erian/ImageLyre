using FluentAssertions;
using ImageLyric.Services.Importer;
using Xunit;

namespace UTest.ServicesTest.Importer
{
    public class ImportTests
    {
        [Fact()]
        public void BaseTest()
        {
            var result = Import.hello(1, 2, 3);
            result.Should().Be(6);
        }
    }
}