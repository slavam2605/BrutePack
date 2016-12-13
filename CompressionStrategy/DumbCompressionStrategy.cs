using System;
using BrutePack.Decompression;
using BrutePack.FileFormat;

namespace BrutePack.CompressionStrategy
{
    public class DumbCompressionStrategy : ICompressionStrategy
    {
        public BrutePackBlock? CompressBlock(byte[] data, int length)
        {
            if(length > 65535 || length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            byte[] newData = new byte[length];
            Array.Copy(data, newData, length);
            return new BrutePackBlock(BlockType.Uncompressed, newData);
        }
    }

    [DecompressionProvider(BlockType.Uncompressed)]
    public class DumbDecompressor : IDecompressionProvider
    {
        public byte[] Decompress(BrutePackBlock block)
        {
            return block.BlockData;
        }
    }
}