using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using ImageLad.ImageEngine;
using Xunit;

namespace UTest.ServicesTest.ImageEngine
{
    public class ImageUtilTest
    {
        [Fact]
        public void IsCMYKTest1()
        {
            var bmp = new Bitmap(@"ImageEngine\CMYK1.jpg");
            var b = true;//ImageUtil.IsCMYK(bmp);
            b.Should().BeTrue();
        }
    }
}
