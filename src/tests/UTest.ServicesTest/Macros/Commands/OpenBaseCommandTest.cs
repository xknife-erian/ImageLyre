using FluentAssertions;
using ImageLaka;
using ImageLaka.Services.Macros.Commands;
using Xunit;

namespace UTest.ServicesTest.Macros.Commands
{
    public class OpenBaseCommandTest
    {
        [Fact]
        public void DoTest1()
        {
            ITarget img = null;
            var cmd = new OpenBeat(img);
            cmd.Execute();
            img.Should().NotBeNull();
        }
    }
}