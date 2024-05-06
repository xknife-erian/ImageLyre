using System;

namespace ImageLyre.ImageEngine.LookUpTable
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
