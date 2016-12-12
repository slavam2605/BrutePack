using System;
using System.IO;
using System.Linq;
using System.Text;
using BrutePack.GZip;
using NUnit.Framework;

namespace BrutePack_Tests.GZip
{
    [TestFixture]
    public class GZipTest
    {
        [Test]
        public void TestBasicGZipRoundtrip1()
        {
            const string testString = "aaacb";
            var data = Encoding.UTF8.GetBytes(testString);
            var dataStream = new MemoryStream(data, 0, data.Length);

            var encodedDataStream = new MemoryStream();
            GZipCompressor.Compress(dataStream, encodedDataStream);
            var encodedData = encodedDataStream.ToArray();
            var decodedDataStream = new MemoryStream();
            encodedDataStream.Seek(0, SeekOrigin.Begin);
            GZipDecompressor.Decompress(encodedDataStream, decodedDataStream);
            var decodedData = decodedDataStream.ToArray();

            var resultString = Encoding.UTF8.GetString(decodedData);
            Console.WriteLine("{0} {1} {2}", data.Length, encodedData.Length, decodedData.Length);
            Console.WriteLine(encodedData.Aggregate("", (s, b) => s + ", " + b));
            Console.WriteLine(testString);
            Console.WriteLine(resultString);
            Assert.AreEqual(testString, resultString);
        }

        [Test]
        public void TestBasicGZipRoundtrip2()
        {
            const string testString = "ユチエフ-8は素晴らしいです";
            var data = Encoding.UTF8.GetBytes(testString);
            var dataStream = new MemoryStream(data, 0, data.Length);

            var encodedDataStream = new MemoryStream();
            GZipCompressor.Compress(dataStream, encodedDataStream);
            var encodedData = encodedDataStream.ToArray();
            var decodedDataStream = new MemoryStream();
            encodedDataStream.Seek(0, SeekOrigin.Begin);
            GZipDecompressor.Decompress(encodedDataStream, decodedDataStream);
            var decodedData = decodedDataStream.ToArray();

            var resultString = Encoding.UTF8.GetString(decodedData);
            Console.WriteLine("{0} {1} {2}", data.Length, encodedData.Length, decodedData.Length);
            Console.WriteLine(encodedData.Aggregate("", (s, b) => s + ", " + b));
            Console.WriteLine(testString);
            Console.WriteLine(resultString);
            Assert.AreEqual(testString, resultString);
        }
    }
}