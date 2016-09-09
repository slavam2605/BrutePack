using System;
using System.IO;
using System.Xml.Schema;
using BitStream;
using BrutePack.Compression;
using BrutePack.GZip;

namespace BrutePack
{
    public class BrutePackMain
    {
        public static void Main(string[] args)
        {
            GZipDecompressor.Decompress(
                new FileStream("C:\\Users\\slava\\test.gz", FileMode.Open)
                , null
            );
        }
    }
}
