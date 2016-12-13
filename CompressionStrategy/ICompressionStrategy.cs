using BrutePack.FileFormat;

namespace BrutePack.CompressionStrategy
{
    public interface ICompressionStrategy
    {
        BrutePackBlock? CompressBlock(byte[] data, int length);
    }
}