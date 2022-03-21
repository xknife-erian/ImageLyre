using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace UTest.ServicesTest.ImageEngine
{
    public class ImageReaderTest
    {
        protected readonly ITestOutputHelper Output;

        public ImageReaderTest(ITestOutputHelper tempOutput)
        {
            Output = tempOutput;
        }

        [Fact]
        public void Read()
        {
            var path = @"./ImageEngine/2022-2-17.jpg";
            File.Exists(path).Should().BeTrue();
            var tw = new Stopwatch();
            tw.Start();
            ReadMethod.Read0(path);
            tw.Stop();
            Output.WriteLine(tw.ElapsedMilliseconds.ToString());
            tw.Reset();
            tw.Start();
            ReadMethod.Read1(path);
            tw.Stop();
            Output.WriteLine(tw.ElapsedMilliseconds.ToString());
            tw.Reset();
            tw.Start();
            ReadMethod.Read2(path);
            tw.Stop();
            Output.WriteLine(tw.ElapsedMilliseconds.ToString());
        }
    }
}
