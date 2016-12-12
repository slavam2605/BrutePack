using System;
using System.IO;
using BrutePack.FileFormat;
using BrutePack.FileFormat.CompressionStrategy;

namespace BrutePack.GZip
{
    public class GZipCompressionStrategy : ICompressionStrategy
    {
        public BrutePackBlock? CompressBlock(byte[] data, int length)
        {
            var input = new MemoryStream(data, 0, length);
            var output = new MemoryStream();
            GZipCompressor.Compress(input, output);
            try
            {
                return new BrutePackBlock(BlockType.GZip, output.ToArray());
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

    }
}