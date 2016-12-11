namespace BrutePack.FileFormat.CompressionStrategy
{
    public interface ICompressionStrategy
    {
        BrutePackBlock? CompressBlock(byte[] data, int length);
    }
}