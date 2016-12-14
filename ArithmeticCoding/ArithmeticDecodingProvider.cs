using System.IO;
using BrutePack.Decompression;
using BrutePack.FileFormat;

namespace BrutePack.ArithmeticCoding
{
    [DecompressionProvider(BlockType.Arithmetic)]
    public class ArithmeticDecodingProvider : IDecompressionProvider
    {
        public byte[] Decompress(BrutePackBlock block)
        {
            return ArithmeticCoder.DecodeBlockStream(new MemoryStream(block.BlockData));
        }
    }
}