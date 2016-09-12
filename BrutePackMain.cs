using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BrutePack.GZip;

namespace BrutePack
{
    public class BrutePackMain
    {
        public static void Main(string[] args)
        {
            try
            {
                GZipDecompressor.Decompress(
                    new FileStream("C:\\Users\\slava\\test.gif.gz", FileMode.Open)
                    , new FileStream("C:\\Users\\slava\\test.gif", FileMode.Create)
                );
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
