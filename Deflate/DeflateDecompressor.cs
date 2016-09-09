using System;
using System.IO;
using BitStream;

namespace BrutePack.Deflate
{
    public class DeflateDecompressor
    {
        public static void Decompress(Stream input, Stream output)
        {
            var bitStream = new BitStream.BitStream(input);
            byte bfinal;
            do
            {
                byte btype;
                bitStream.ReadBits(out bfinal, (BitNum) 1);
                bitStream.ReadBits(out btype, (BitNum) 2);
                Console.WriteLine(bfinal == 0 ? "NOT_FINAL_BLOCK" : "FINAL_BLOCK");
                Console.WriteLine(
                    btype == 0 ? "NOT_COMPRESSED" : btype == 1 ? "STATIC_HUFFMAN" : "DYNAMIC_HUFFMAN"
                );
            } while (bfinal == 0);
        }
    }
}