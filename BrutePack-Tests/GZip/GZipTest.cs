using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using BrutePack.Deflate;
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

        [Test]
        public void TestGZipCompressDecompress()
        {
            var input = new MemoryStream(
                GetBytes("some test string for gzip compression lalala ∀ε ∃ δ: |a - x| → 0")
            );
            var buffer = new byte[1024];
            var count = input.Read(buffer, 0, 1024);
            var compressed = new GZipCompressionStrategy().CompressBlock(buffer, count);
            Assert.IsTrue(compressed.HasValue);
            var decompressed = new GZipDecompressionProvider().Decompress(compressed.Value);
            CollectionAssert.AreEqual(buffer.Take(count), decompressed);
        }

        [Test]
        public void TestCopyDeflateCompress()
        {
            var input = new MemoryStream(
                GetBytes("some test string for gzip compression lalala ∀ε ∃ δ: |a - x| → 0")
            );
            var output = new MemoryStream();
            CopyDeflateCompressor.Compress(input, output);
            var rawInput = input.ToArray();
            var rawOutput = output.ToArray();
            Assert.AreEqual(rawOutput[0], 1);
            Assert.AreEqual(rawOutput[1], rawInput.Length & 0xFF);
            Assert.AreEqual(rawOutput[2], rawInput.Length >> 8);
            Assert.AreEqual(rawOutput[3], (byte) ~rawOutput[1]);
            Assert.AreEqual(rawOutput[4], (byte) ~rawOutput[2]);
            CollectionAssert.AreEqual(rawInput, rawOutput.Skip(5));
        }

        [Test]
        public void TestCompliance1()
        {
            var path1 = TestUtil.GetTestDataDir() + "file1.gz";
            var path2 = TestUtil.GetTestDataDir() + "file1";
            var input = new FileStream(path1, FileMode.Open);
            var output = new MemoryStream();
            GZipDecompressor.Decompress(input, output);
            var inputNotCompressed = new FileStream(path2, FileMode.Open);
            var rawData = new byte[inputNotCompressed.Length];
            var count = inputNotCompressed.Read(rawData, 0, rawData.Length);
            var uncompressed = output.ToArray();
            CollectionAssert.AreEqual(rawData.Take(count), uncompressed);
        }

        [Test]
        public void TestCompliance2()
        {
            var path1 = TestUtil.GetTestDataDir() + "file2.gz";
            var path2 = TestUtil.GetTestDataDir() + "file2";
            var input = new FileStream(path1, FileMode.Open);
            var output = new MemoryStream();
            GZipDecompressor.Decompress(input, output);
            int count;
            var rawData = ReadFile(path2, out count);
            var uncompressed = output.ToArray();
            CollectionAssert.AreEqual(rawData.Take(count), uncompressed);
        }

        [Test]
        public void TestSelfCompliance()
        {
            var path1 = TestUtil.GetTestDataDir() + "file";
            var path2 = TestUtil.GetTestDataDir() + "file.gz";
            var path3 = TestUtil.GetTestDataDir() + "outFile";
            var stream1 = new FileStream(path1, FileMode.Open);
            var stream2 = new FileStream(path2, FileMode.Create);
            GZipCompressor.Compress(stream1, stream2);
            stream2.Seek(0, SeekOrigin.Begin);
            var stream3 = new FileStream(path3, FileMode.Create);
            GZipDecompressor.Decompress(stream2, stream3);
            stream1.Close();
            stream2.Close();
            stream3.Close();
            int count1, count2;
            var buffer1 = ReadFile(path1, out count1);
            var buffer2 = ReadFile(path3, out count2);
            CollectionAssert.AreEqual(buffer1.Take(count1), buffer2.Take(count2));
        }

        private static byte[] ReadFile(string path2, out int count)
        {
            var inputNotCompressed = new FileStream(path2, FileMode.Open);
            var rawData = new byte[inputNotCompressed.Length];
            count = inputNotCompressed.Read(rawData, 0, rawData.Length);
            inputNotCompressed.Close();
            return rawData;
        }

        private static byte[] GetBytes(string str)
        {
            var bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}