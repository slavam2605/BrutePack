using System;
using System.IO;
using BrutePack.ExternalCompressor;
using BrutePack.FileFormat;
using BrutePack.Util;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace BrutePack_Tests.ExternalCompression
{
    [TestFixture]
    public class ExternalCompressorTest
    {
        private const int TestBlockSize = 320000;

        private void IgnoreIfNoCommand(string command)
        {
            if(!FileUtil.ExistsOnPath(command))
                Assert.Ignore("{0} is not present on system", command);
        }

        [Test]
        public void TestCompressionWithCat()
        {
            IgnoreIfNoCommand("cat");
            TestCompressionWithConfig(new ExternalCompressorConfig("cat", "cat"), false);
        }

        [Test]
        public void TestCompressionWithGzip()
        {
            IgnoreIfNoCommand("gzip");
            TestCompressionWithConfig(new ExternalCompressorConfig("gzip -9 -", "gzip -d -"), true);
        }


        public void TestCompressionWithConfig(ExternalCompressorConfig externalCompressorConfig, bool checkReduction)
        {
            var catCompressionConfig = externalCompressorConfig;
            var strategy = new ExternalCompressionStrategy(catCompressionConfig);

            var data = new byte[TestBlockSize];
            for (int i = 0; i < TestBlockSize; i++)
            {
                data[i] = (byte) (i * 5);
            }

            var memStream = new MemoryStream();
            var compressingStream = new BruteCompressingStream(new BinaryWriter(memStream), 65000, strategy);
            compressingStream.Write(data, 0, TestBlockSize);
            compressingStream.Flush();

            if (checkReduction)
            {
                Assert.Less(memStream.Position, TestBlockSize);
                Console.WriteLine("Compressed {0} to {1} ({2}%)", TestBlockSize, memStream.Position,
                    memStream.Position * 100 / TestBlockSize);
            }
            memStream.Seek(0, SeekOrigin.Begin);
            var decompressingStream = new BruteUncompressingStream(new BinaryReader(memStream));
            var decompressingReader = new BinaryReader(decompressingStream);

            var readBlock = decompressingReader.ReadBytes(TestBlockSize);

            Assert.AreEqual(data, readBlock);
        }
    }
}