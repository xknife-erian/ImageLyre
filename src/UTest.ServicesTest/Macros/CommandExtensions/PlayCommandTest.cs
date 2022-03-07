using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using ImageLaka.Services.Macros;
using Xunit;

namespace UTest.ServicesTest.Macros.CommandExtensions
{
    public class PlayCommandTest
    {
        [Fact]
        public void Test1()
        {
            var tt = new TextTarget();
            var macro = new Macro();
            macro.Add(new PlayCommand(tt));
            macro.Do();
            tt.TestTarget.Should().Be("Play/");
        }
    }
}
