using System;
using System.IO;
using BrutePack.CompressionStrategy;
using BrutePack.ExternalCompressor;
using BrutePack.FileFormat;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace BrutePack_Tests.BruteStrategy
{
    [TestFixture]
    public class BruteStrategyTest
    {
        private class MockStrategy : ICompressionStrategy
        {
            private static readonly byte[] Empty = new byte[0];
            private static readonly byte[] Full = new byte[5];
            private readonly byte target;

            public MockStrategy(byte target)
            {
                this.target = target;
            }

            public BrutePackBlock? CompressBlock(byte[] data, int length)
            {
                return new BrutePackBlock(BlockType.Uncompressed, data[0] == target ? Empty : Full);
            }
        }

        private const int TestSize = 128;

        [Test]
        public void TestSelectionStrategy()
        {
            var data = new byte[TestSize];
            for (int i = 0; i < TestSize; i++)
                data[i] = (byte) i;

            var compressors = new ICompressionStrategy[TestSize];
            for (int i = 0; i < TestSize; i++)
                compressors[i] = new MockStrategy((byte) i);

            var strategy = new BruteCompressionStrategy(compressors);

            var memStream = new MemoryStream();
            var compressingStream = new BruteCompressingStream(new BinaryWriter(memStream), 1, strategy);
            compressingStream.Write(data, 0, TestSize);
            compressingStream.Flush();

            Assert.AreEqual(memStream.Position, TestSize * 3);
            Console.WriteLine("Compressed {0} to {1} ({2}%)", TestSize, memStream.Position,
                memStream.Position * 100 / TestSize);
        }
    }
}
