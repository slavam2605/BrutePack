using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using BrutePack.ArithmeticCoding;
using BrutePack.ExternalCompressor;
using BrutePack.FileFormat;
using NUnit.Framework;

namespace BrutePack_Tests.Arithmetic
{
    [TestFixture]
    public class ArithmeticCodingTest
    {
        [Test]
        public void TestBasicArithmeticCodingRoundtrip1()
        {
            const string testString = "aaacb";
            var data = Encoding.UTF8.GetBytes(testString);
            var frequencies = ArithmeticCoder.CalculateFrequencies(data);
            var encodedData = ArithmeticCoder.Encode(data, frequencies);
            var decodedData = ArithmeticCoder.Decode(encodedData, frequencies, data.Length);
            var resultString = Encoding.UTF8.GetString(decodedData);
            Console.WriteLine("{0} {1} {2}", data.Length, encodedData.Length, decodedData.Length);
            Console.WriteLine(encodedData.Aggregate("", (s, b) => s + ", " + b));
            Console.WriteLine(testString);
            Console.WriteLine(resultString);
            Assert.LessOrEqual(encodedData.Length, data.Length);
            Assert.AreEqual(testString, resultString);
        }

        [Test]
        public void TestBasicArithmeticCodingRoundtrip2()
        {
            const string testString = "ユチエフ-8は素晴らしいです";
            var data = Encoding.UTF8.GetBytes(testString);
            var frequencies = ArithmeticCoder.CalculateFrequencies(data);
            var encodedData = ArithmeticCoder.Encode(data, frequencies);
            var decodedData = ArithmeticCoder.Decode(encodedData, frequencies, data.Length);
            var resultString = Encoding.UTF8.GetString(decodedData);
            Console.WriteLine("{0} {1} {2}", data.Length, encodedData.Length, decodedData.Length);
            Console.WriteLine(encodedData.Aggregate("", (s, b) => s + ", " + b));
            Console.WriteLine(testString);
            Console.WriteLine(resultString);
            Assert.LessOrEqual(encodedData.Length, data.Length);
            Assert.AreEqual(testString, resultString);
        }

        [Test]
        public void TestArithmeticStreamsRoundtrip1()
        {
            const string testString = "ユチエフ-8は素晴らしいです";
            var data = Encoding.UTF8.GetBytes(testString);
            var originalStream = new MemoryStream(data, false);
            var encodedStream = new MemoryStream();
            ArithmeticCoder.EncodeStream(originalStream, encodedStream);
            var decodedStream = new MemoryStream();
            encodedStream.Position = 0;
            ArithmeticCoder.DecodeStream(encodedStream, decodedStream);
            var decodedData = decodedStream.ToArray();
            var encodedData = encodedStream.ToArray();
            var resultString = Encoding.UTF8.GetString(decodedData);
            Console.WriteLine("{0} {1} {2}", data.Length, encodedData.Length, decodedData.Length);
            Console.WriteLine(encodedData.Aggregate("", (s, b) => s + ", " + b));
            Console.WriteLine(testString);
            Console.WriteLine(resultString);
            Assert.LessOrEqual(encodedData.Length, data.Length + 258);
            Assert.AreEqual(testString, resultString);
        }

        [Test]
        [Ignore("Too slow, missing data")]
        public void LargeFileTest1()
        {
            var path = TestUtil.GetTestDataDir(TestPathType.ClassName) + "test.wav";
            var inStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var outStream = new MemoryStream();
            var stopwatch = Stopwatch.StartNew();
            ArithmeticCoder.EncodeStream(inStream, outStream, 65536, 16);
            var endpoint = stopwatch.Elapsed;
            Console.WriteLine(endpoint);
            Console.WriteLine("Encoded size is {0}", outStream.Position);
        }

        [Test]
        [Ignore("Native library is unstable, investigate further")]
        public void TestCodingStrategy()
        {
            TestCompressionWithConfig(64);
        }

        private const int TestBlockSize = 32;

        public void TestCompressionWithConfig(int size)
        {
            var strategy = new ArithmeticCodingStrategy(size);

            var data = new byte[TestBlockSize];
            for (int i = 0; i < TestBlockSize; i++)
            {
                data[i] = (byte)(i * 128 + 1);
            }

            var memStream = new MemoryStream();
            var compressingStream = new BruteCompressingStream(new BinaryWriter(memStream), TestBlockSize, strategy);
            compressingStream.Write(data, 0, TestBlockSize);
            compressingStream.Flush();

            memStream.Seek(0, SeekOrigin.Begin);
            var decompressingStream = new BruteUncompressingStream(new BinaryReader(memStream));
            var decompressingReader = new BinaryReader(decompressingStream);

            var readBlock = decompressingReader.ReadBytes(TestBlockSize);

            Assert.AreEqual(data, readBlock);
        }
    }
}
