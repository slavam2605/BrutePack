using System.Linq;
using BrutePack.ExternalCompressor;
using BrutePack.GZip;
using BrutePack.StrategyConfig;
using NUnit.Framework;

namespace BrutePack_Tests.ConfigParser
{
    [TestFixture]
    public class ConfigParserTest
    {
        [Test]
        public void TestGzipParsing()
        {
            var list = StrategyConfigParser.ParseConfig("gzip").ToList();

            Assert.AreEqual(1, list.Count);
            Assert.IsInstanceOf<GZipCompressionStrategy>(list[0]);
        }

        [Test]
        public void TestMultiGzipParsing()
        {
            var list = StrategyConfigParser.ParseConfig("gzip;gzip").ToList();

            Assert.AreEqual(2, list.Count);
            Assert.IsInstanceOf<GZipCompressionStrategy>(list[0]);
            Assert.IsInstanceOf<GZipCompressionStrategy>(list[1]);
        }

        [Test]
        public void TestGzipParsingWithSpaces()
        {
            var list = StrategyConfigParser.ParseConfig("   gzip    ").ToList();

            Assert.AreEqual(1, list.Count);
            Assert.IsInstanceOf<GZipCompressionStrategy>(list[0]);
        }

        [Test]
        public void TestExternalParsing()
        {
            var list = StrategyConfigParser.ParseConfig("ext,gzip -,gunzip -").ToList();

            Assert.AreEqual(1, list.Count);
            Assert.IsInstanceOf<ExternalCompressionStrategy>(list[0]);

            var strategy = (ExternalCompressionStrategy) list[0];
            Assert.AreEqual("gzip -", strategy.Config.CompressCommand);
            Assert.AreEqual("gunzip -", strategy.Config.UncompressCommand);

        }
    }
}