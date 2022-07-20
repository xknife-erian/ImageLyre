using System.Windows.Media;
using FluentAssertions;
using ImageLad.UI.Views.Utils;
using Xunit;
using ImagingPixelFormat = System.Drawing.Imaging.PixelFormat;
using MediaPixelFormat = System.Windows.Media.PixelFormat;

namespace UTest.ViewTest.Utils
{
    public class UtilTests
    {
        [Fact]
        public void TranslateBitmapFormatTest()
        {
            var f = Util.TranslateBitmapFormat((ImagingPixelFormat) 8207);
            f.Should().Be(PixelFormats.Cmyk32);
        }
    }
}