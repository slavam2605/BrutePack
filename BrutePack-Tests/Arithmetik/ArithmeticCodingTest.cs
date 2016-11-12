using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using BrutePack.ArithmeticCoding;
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
            ArithmeticCoder.EncodeStream(inStream, outStream, 1024);
            var endpoint = stopwatch.Elapsed;
            Console.WriteLine(endpoint);
            Console.WriteLine("Encoded size is {0}", outStream.Position);
        }
    }
}