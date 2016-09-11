using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using BitStream;
using BrutePack.Huffman;

namespace BrutePack.Deflate
{
    public class DeflateDecompressor
    {
        private static readonly int[] lengthExtraBits =
            {0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0};

        private static readonly int[] lengthStart =
            {3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 15, 17, 19, 23, 27, 31, 35, 43, 51, 59, 67, 83, 99,
                115, 131, 163, 195, 227, 258};

        private static readonly int[] offsetExtraBits =
            {0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13};

        private static readonly int[] offsetStart =
            {1, 2, 3, 4, 5, 7, 9, 13, 17, 25, 33, 49, 65, 97, 129, 193, 257, 385, 513, 769, 1025,
                1537, 2049, 3073, 4097, 6145, 8193, 12289, 16385, 24577};

        public static void Decompress(Stream input, Stream output)
        {
            var bitStream = new BitStream.BitStream(input);
            var data = new List<byte>();
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
                if (btype == 2)
                    throw new NotSupportedException("Dynamic Huffman method is not supported");
                if (btype == 0)
                    throw new NotSupportedException("Not compressed blocks are not supported");
                var decoder = HuffmanTree.StaticTree.GetDecoder();
                while (true)
                {
                    byte bit;
                    if (!bitStream.ReadBits(out bit, (BitNum) 1))
                        break;
                    var value = decoder.Next(bit);
                    if (value < 0) continue;
                    Console.WriteLine(value);
                    if (value < 256)
                    {
                        data.Add((byte) value);
                        continue;
                    }
                    if (value == 256)
                        break;
                    var extraBits = lengthExtraBits[value - 257];
                    var currentPower = 1;
                    var extraValue = 0;
                    for (var i = 0; i < extraBits; i++)
                    {
                        bitStream.ReadBits(out bit, (BitNum) 1);
                        extraValue += currentPower * bit;
                        currentPower <<= 1;
                    }
                    var length = lengthStart[value - 257] + extraValue;
                    int offset;
                    bitStream.ReadBits(out bit, (BitNum) 5);
                    bit = (byte) ((bit >> 4) + ((bit & 8) >> 2) + (bit & 4) + ((bit & 2) << 1) + ((bit & 1) << 2));
                    offset = offsetStart[bit]; // TODO maybe 5-reverse bit
                    extraBits = offsetExtraBits[offset];
                    currentPower = 1;
                    extraValue = 0;
                    for (var i = 0; i < extraBits; i++)
                    {
                        bitStream.ReadBits(out bit, (BitNum) 1);
                        extraValue += currentPower * bit;
                        currentPower <<= 1;
                    }
                    offset += extraValue;
                    Console.WriteLine("<" + length + ", " + offset + ">");
                    for (var i = 0; i < length; i++)
                    {
                        data.Add(data[data.Count - offset]);
                    }
                }
            } while (bfinal == 0);
            Console.Write("decompressed=\"");
            foreach (var aByte in data)
            {
                Console.Write((char) aByte);
            }
            Console.WriteLine("\"");
        }
    }
}