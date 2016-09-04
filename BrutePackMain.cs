using System;
using System.Diagnostics;
using System.IO;
using BrutePack.Compression;

namespace BrutePack
{
    public class BrutePackMain
    {
        private const int WARM_UP_ITERS = 2;
        private const int TEST_ITERS = 10;

        private static void Test()
        {
            Stream input = new FileStream("test", FileMode.Open);
            Stream output = new FileStream("test.compressed", FileMode.Create);
            new DummyCompressor().Compress(input, output);
            input.Close();
            output.Close();
        }

        public static void Main(string[] args)
        {
            for (var i = 0; i < WARM_UP_ITERS; i++)
                Test();
            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < TEST_ITERS; i++)
                Test();
            Console.WriteLine(stopwatch.ElapsedMilliseconds / TEST_ITERS);
        }
    }
}
