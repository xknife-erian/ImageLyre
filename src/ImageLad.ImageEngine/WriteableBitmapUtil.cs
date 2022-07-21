using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ImageLad.ImageEngine
{
    public class WriteableBitmapUtil
    {
        /// <summary>
        ///     从流(<see cref="FileStream" />,<see cref="MemoryStream" />)快速创建<see cref="WriteableBitmap" />
        /// </summary>
        /// <param name="stream">包含图像的流</param>
        /// <returns>WPF界面的<see cref="BitmapSource" /></returns>
        public static WriteableBitmap BuildBitmap(Stream stream)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return new WriteableBitmap(bitmapImage);
        }
    }
}
