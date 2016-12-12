using System.IO;
using BrutePack.Decompression;
using BrutePack.FileFormat;

namespace BrutePack.GZip
{
    public class GZipDecompressionProvider : IDecompressionProvider
    {
        [DecompressionProvider(BlockType.GZip)]
        public byte[] Decompress(BrutePackBlock block)
        {
            var input = new MemoryStream(block.BlockData, 0, block.BlockData.Length);
            var output = new MemoryStream();
            GZipDecompressor.Decompress(input, output);
            return output.ToArray();
        }
    }
}