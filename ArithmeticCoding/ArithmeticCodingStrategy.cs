using System.IO;
using BrutePack.CompressionStrategy;
using BrutePack.FileFormat;

namespace BrutePack.ArithmeticCoding
{
    public class ArithmeticCodingStrategy : ICompressionStrategy
    {
        private readonly int innerChunkSize;

        public ArithmeticCodingStrategy(int innerChunkSize)
        {
            this.innerChunkSize = innerChunkSize;
        }

        public BrutePackBlock? CompressBlock(byte[] data, int length)
        {
            var memStream = new MemoryStream();
            ArithmeticCoder.EncodeBlockStream(data, memStream, length, innerChunkSize);
            if (memStream.Position >= 1024 * 1024 * 2)
                return null;
            return new BrutePackBlock(BlockType.Arithmetic, memStream.ToArray());
        }
    }
}