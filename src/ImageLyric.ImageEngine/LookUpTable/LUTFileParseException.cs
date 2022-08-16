using System;

namespace ImageLyric.ImageEngine.LookUpTable
{
    public class LUTFileParseException : Exception
    {
        public LUTFileParseException()
        {
        }
    }

    public class EOFException : Exception
    {
    }
}
