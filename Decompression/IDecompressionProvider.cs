using BrutePack.FileFormat;

namespace BrutePack.Decompression
{
    public interface IDecompressionProvider
    {
        byte[] Decompress(BrutePackBlock block);
    }
}
