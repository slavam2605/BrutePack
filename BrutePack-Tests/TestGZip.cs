using System.IO;
using BrutePack.GZip;
using NUnit.Framework;

namespace BrutePack_Tests
{
    [TestFixture]
    public class TestGZip
    {
        [Test]
        public void RunSpeed()
        {
            GZipDecompressor.Decompress(
                new FileStream(TestUtil.GetTestDataDir() + "test.tar.gz", FileMode.Open)
                , new FileStream(TestUtil.GetTestDataDir() + "test.tar", FileMode.Create)
            );
        }
    }
}