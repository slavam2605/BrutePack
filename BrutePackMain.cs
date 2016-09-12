using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using BrutePack.BitStream;
using BrutePack.GZip;
using BrutePack.Huffman;

namespace BrutePack
{
    public class BrutePackMain
    {
        public static void Main(string[] args)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                GZipDecompressor.Decompress(
                    new FileStream("C:\\Users\\slava\\RiderProjects\\BrutePack\\BrutePack-Tests\\TestData\\TestGZip\\out.gz", FileMode.Open)
                    , new FileStream("C:\\Users\\slava\\RiderProjects\\BrutePack\\BrutePack-Tests\\TestData\\TestGZip\\out", FileMode.Create)
                );
                Console.Out.WriteLine("Time elapsed: " + stopwatch.Elapsed);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
