using System.IO;
using BrutePack.CompressionStrategy;
using BrutePack.FileFormat;

namespace BrutePack.ArithmeticCoding
{
    public class ArithmeticCodingStrategy : ICompressionStrategy
    {
        public int InnerChunkSize { get; }

        public ArithmeticCodingStrategy(int innerChunkSize)
        {
            this.InnerChunkSize = innerChunkSize;
        }

        public BrutePackBlock? CompressBlock(byte[] data, int length)
        {
            var memStream = new MemoryStream();
            ArithmeticCoder.EncodeBlockStream(data, memStream, length, InnerChunkSize);
            if (memStream.Position >= 1024 * 1024 * 2)
                return null;
            return new BrutePackBlock(BlockType.Arithmetic, memStream.ToArray());
        }
    }
}