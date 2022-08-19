using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using BitMiracle.LibTiff.Classic;

namespace ImageLyre.ImageEngine.Undetermined
{
    internal class TiffClass1
    {
        static void Main(string[] args)
        {
            // read bytes of an image
            byte[] buffer = File.ReadAllBytes(@"C:\Data\multipage.tif");
            List<int> pageNumbers = new List<int>() { 3, 1 };
            var fileBytes = CopySpecifyPagesToNewTiffFile(buffer, pageNumbers);

            FileStream destFileStream = new FileStream(@"C:\Data\multipageNew.tif", FileMode.Create, FileAccess.ReadWrite);
            destFileStream.Write(fileBytes, 0, fileBytes.Length);
            destFileStream.Flush();
            destFileStream.Close();
        }

        //https://github.com/BitMiracle/libtiff.net/blob/master/Samples/ConvertToSingleStripInMemory/C%23/ConvertToSingleStripInMemory.cs
        private static byte[] CopySpecifyPagesToNewTiffFile(byte[] sourceTiffImage, List<int> pageNumbers)
        {
            if (pageNumbers == null || !pageNumbers.Any())
                throw new ArgumentNullException("pageNumbers is null or empty");
            pageNumbers.Sort();
            if (pageNumbers.First() < 1)
                throw new ArgumentException("the minum pageNumbers can't less than 0.");

            // create a memory stream out of them
            MemoryStream ms = new MemoryStream(sourceTiffImage);

            // open a Tiff stored in the memory stream
            using (Tiff image = Tiff.ClientOpen("in-memory", "r", ms, new TiffStream()))
            {
                var numberOfPages = image.NumberOfDirectories();
                if (pageNumbers.Last() - 1 > numberOfPages)
                    throw new ArgumentException("the maxium pageNumbers can't great than max pageNumber");

                using (MemoryStream msOutput = new MemoryStream())
                {
                    using (Tiff output = Tiff.ClientOpen("in-memory", "w", msOutput, new TiffStream()))
                    {
                        int index = 0;
                        foreach (int pageNumber in pageNumbers)
                        {
                            var pageIndex = Convert.ToInt16(pageNumber - 1);
                            image.SetDirectory(pageIndex);

                            copyTags(image, output);
                            // specify that it's a page within the multipage file
                            output.SetField(TiffTag.SUBFILETYPE, FileType.PAGE);
                            // specify the page number
                            output.SetField(TiffTag.PAGENUMBER, index++, numberOfPages);
                            copyStrips(image, output);

                            output.WriteDirectory();
                        }
                    }
                    var bytes = msOutput.ToArray();
                    return bytes;
                }
            }
        }
        private static void copyTags(Tiff input, Tiff output)
        {
            for (ushort t = ushort.MinValue; t < ushort.MaxValue; ++t)
            {
                TiffTag tag = (TiffTag)t;
                FieldValue[] tagValue = input.GetField(tag);
                if (tagValue != null)
                    output.GetTagMethods().SetField(output, tag, tagValue);
            }

            int height = input.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
            output.SetField(TiffTag.ROWSPERSTRIP, height);
        }

        private static void copyStrips(Tiff input, Tiff output)
        {
            bool encoded = false;
            FieldValue[] compressionTagValue = input.GetField(TiffTag.COMPRESSION);
            if (compressionTagValue != null)
                encoded = (compressionTagValue[0].ToInt() != (int)Compression.NONE);

            int numberOfStrips = input.NumberOfStrips();

            int offset = 0;
            byte[] stripsData = new byte[numberOfStrips * input.StripSize()];
            for (int i = 0; i < numberOfStrips; ++i)
            {
                int bytesRead = readStrip(input, i, stripsData, offset, encoded);
                offset += bytesRead;
            }

            writeStrip(output, stripsData, offset, encoded);
        }

        private static int readStrip(Tiff image, int stripNumber, byte[] buffer, int offset, bool encoded)
        {
            if (encoded)
                return image.ReadEncodedStrip(stripNumber, buffer, offset, buffer.Length - offset);
            else
                return image.ReadRawStrip(stripNumber, buffer, offset, buffer.Length - offset);
        }

        private static void writeStrip(Tiff image, byte[] stripsData, int count, bool encoded)
        {
            if (encoded)
                image.WriteEncodedStrip(0, stripsData, count);
            else
                image.WriteRawStrip(0, stripsData, count);
        }

        public void ReadTiff(string fileName, out Tiff tiff, out List<byte[]> listData, out Size size)
        {
            tiff = null;
            listData = new List<byte[]>();
            size = Size.Empty;
            Tiff tif = Tiff.Open(fileName, "r");
            if (tif == null)
                return;
            FieldValue[] value = tif.GetField(TiffTag.IMAGEWIDTH);
            size.Width = value[0].ToInt();
            value = tif.GetField(TiffTag.IMAGELENGTH);
            size.Height = value[0].ToInt();
            value = tif.GetField(TiffTag.BITSPERSAMPLE);
            int bits = value[0].ToInt();
            short dirNumb = tif.NumberOfDirectories();
            listData.Clear();
            for (short i = 0; i < dirNumb; i++)
            {
                tif.SetDirectory(i);
                int lineSize = tif.ScanlineSize();
                byte[] rowData = new byte[lineSize];
                byte[] data = new byte[size.Height * size.Width * (bits / 8)];
                for (int j = 0; j < size.Height; j++)
                {
                    tif.ReadScanline(rowData, j);
                    rowData.CopyTo(data, j * lineSize);
                }

                listData.Add(data);
            }

            tif.Close();
        }

        public void WriteTiff(List<byte[]> listData, Size size)
        {
            Tiff tif = Tiff.Open(@"res.tif", "w");
            for (short i = 0; i < listData.Count; i++)
            {
                tif.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                tif.SetField(TiffTag.IMAGEWIDTH, size.Width);//设置图片宽度
                tif.SetField(TiffTag.IMAGELENGTH, size.Height);//设置图片高度
                tif.SetField(TiffTag.COMPRESSION, Compression.NONE);
                tif.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                tif.SetField(TiffTag.BITSPERSAMPLE, 16);//设置图片数据位数
                tif.SetField(TiffTag.SAMPLESPERPIXEL, 1);//设置图片通道数
                tif.WriteRawStrip(0, listData[i], listData[i].Length);
                tif.WriteDirectory();
            }
            tif.Close();
        }

    }
}
