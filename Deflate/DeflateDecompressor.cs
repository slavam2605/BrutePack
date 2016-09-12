using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrutePack.BitStream;
using BrutePack.DataStructure;
using BrutePack.Huffman;

namespace BrutePack.Deflate
{
    public class DeflateDecompressor
    {
        private static readonly int[] lengthExtraBits =
            {0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0};

        private static readonly int[] lengthStart =
        {
            3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 15, 17, 19, 23, 27, 31, 35, 43, 51, 59, 67, 83, 99,
            115, 131, 163, 195, 227, 258
        };

        private static readonly int[] offsetExtraBits =
            {0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13};

        private static readonly int[] offsetStart =
        {
            1, 2, 3, 4, 5, 7, 9, 13, 17, 25, 33, 49, 65, 97, 129, 193, 257, 385, 513, 769, 1025,
            1537, 2049, 3073, 4097, 6145, 8193, 12289, 16385, 24577
        };

        private static readonly int[] lengthsOrder =
            {16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15};

        private static int FetchValue(BitReader stream, HuffmanTree.HuffmanDecoder decoder)
        {
            while (true)
            {
                var bit = stream.ReadBit();
                var value = decoder.Next(bit);
                if (value >= 0)
                    return value;
            }
        }

        private static byte[] ReadBytes(Stream stream, int count)
        {
            var buffer = new byte[count];
            var read = 0;
            while (read < count)
            {
                var result = stream.Read(buffer, read, count - read);
                if (result == 0)
                    break;
                if (result < 0)
                    throw new ApplicationException("wtf");
                read += result;
            }
            return buffer;
        }

        public static int[] literalTreeArray = new int[65536];
        public static int[] offsetTreeArray = new int[65536];

        public static void Decompress(Stream input, Stream output)
        {
            var bitStream = new BitReader(input);
            var data = new List<byte>();
            byte bfinal;
            do
            {
                bfinal = bitStream.ReadBit();
                var btype = bitStream.ReadBits(2);
//                Console.WriteLine(
//                    (bfinal == 0 ? "NOT_FINAL_BLOCK" : "FINAL_BLOCK") +
//                    " : " +
//                    (btype == 0 ? "NOT_COMPRESSED" : btype == 1 ? "STATIC_HUFFMAN" : "DYNAMIC_HUFFMAN")
//                );
                switch (btype)
                {
                    case 0:
                        DecompressCopy(bitStream, data);
                        break;
                    case 1:
                        DecompressStatic(bitStream, data);
                        break;
                    case 2:
                        DecompressDynamic(bitStream, data);
                        break;
                    default:
                        throw new ArgumentException("Invalid compression method, BTYPE = " + btype);
                }
            } while (bfinal == 0);
            output.Write(data.ToArray(), 0, data.Count);
//            Console.Write("decompressed=\"");
//            foreach (var aByte in data)
//            {
//                Console.Write((char) aByte);
//            }
//            Console.WriteLine("\"");
        }

        private static void DecompressCopy(BitReader bitStream, IList<byte> data)
        {
            bitStream.FinishByte();
            var byte1 = bitStream.ReadByte();
            var byte2 = bitStream.ReadByte();
            var len = byte1 + (byte2 << 8);
            bitStream.Read(new byte[2], 0, 2); // skip 2 bytes
            var buffer = ReadBytes(bitStream, len);
            if (data.GetType() == typeof(BlockList<byte>))
                ((BlockList<byte>) data).AddArray(buffer);
            else if (data.GetType() == typeof(List<byte>))
                ((List<byte>) data).AddRange(buffer);
            else
                foreach (var b in data)
                {
                    data.Add(b);
                }
        }

        private static void DecompressDynamic(BitReader bitStream, IList<byte> data)
        {
//            while (true)
//            {
//                byte bit;
//                if (!bitStream.ReadBits(out bit, 1))
//                    break;
//                Console.Write(bit);
//            }
//            return;

            var lengthTree = new HuffmanTree(new int[512], 512);

            var hlit = 257 + bitStream.ReadBits(5);
            var hdist = 1 + bitStream.ReadBits(5);
            var hclen = 4 + bitStream.ReadBits(4);

            var lengthLengths = new int[19];
            for (var i = 0; i < hclen; i++)
            {
                lengthLengths[lengthsOrder[i]] = bitStream.ReadBits(3);
            }
            GenCodes(lengthLengths, lengthTree);

            var literalLengths = new int[287];
            ReadLengths(bitStream, lengthTree, literalLengths, hlit);
            var literalTree = new HuffmanTree(literalTreeArray, 1 << literalLengths.Max() + 2);
            GenCodes(literalLengths, literalTree);

            var offsetLengths = new int[32];
            ReadLengths(bitStream, lengthTree, offsetLengths, hdist);
            var offsetTree = new HuffmanTree(offsetTreeArray, 1 << offsetLengths.Max() + 2);
            GenCodes(offsetLengths, offsetTree);

            var literalDecoder = literalTree.GetDecoder();
            var offsetDecoder = offsetTree.GetDecoder();
            while (true)
            {
                var bit = bitStream.ReadBit();
                var value = literalDecoder.Next(bit);
                if (value < 0) continue;
                if (value < 256)
                {
                    data.Add((byte) value);
                    continue;
                }
                if (value == 256) break;
                var length = lengthStart[value - 257] + bitStream.ReadBits(lengthExtraBits[value - 257]);
                var offsetId = FetchValue(bitStream, offsetDecoder);
                var offset = offsetStart[offsetId];
                offset += bitStream.ReadBits(offsetExtraBits[offsetId]);
                for (var i = 0; i < length; i++)
                {
                    data.Add(data[data.Count - offset]);
                }
            }
        }

        private static void ReadLengths(BitReader stream, HuffmanTree tree, int[] lengths, int count)
        {
            var lengthsDecoder = tree.GetDecoder();
            var readLiterals = 0;
            while (readLiterals < count)
            {
                var bit = stream.ReadBit();
                var value = lengthsDecoder.Next(bit);
                if (value < 0) continue;
                if (value < 16)
                {
                    lengths[readLiterals++] = value;
                    continue;
                }
                switch (value)
                {
                    case 16:
                    {
                        var extraBits = 3 + stream.ReadBits(2);
                        for (var i = 0; i < extraBits; i++)
                        {
                            lengths[readLiterals + i] = lengths[readLiterals - 1];
                        }
                        readLiterals += extraBits;
                        break;
                    }
                    case 17:
                    {
                        var extraBits = stream.ReadBits(3);
                        readLiterals += 3 + extraBits;
                        break;
                    }
                    case 18:
                    {
                        var extraBits = stream.ReadBits(7);
                        readLiterals += 11 + extraBits;
                        break;
                    }
                    default:
                        throw new FormatException("Unknown value: " + value);
                }
            }
        }

        private static void GenCodes(int[] lengths, HuffmanTree tree)
        {
            var codes = GetCodes(lengths);
            for (var i = 0; i < lengths.Length; i++)
            {
                var length = lengths[i];
                if (length == 0)
                    continue;
                var code = codes[length]++;
                tree.AddCode(ToFixedBinaryString(code, length), i);
            }
        }

        private static string ToFixedBinaryString(int x, int length)
        {
            var s = Convert.ToString(x, 2);
            if (s.Length > length)
                throw new ArgumentException("Cannot fit a big number into a little string: " + x + "[" + s + "]:" +
                                            length);
            while (s.Length < length)
                s = "0" + s;
            return s;
        }

        private static int[] GetCodes(int[] lengths)
        {
            var maxLength = lengths.Max();
            var blCount = new int[maxLength + 1];
            foreach (var length in lengths)
            {
                if (length != 0)
                    blCount[length]++;
            }
            var code = 0;
            var firstCode = new int[maxLength + 1];
            for (var bits = 1; bits <= maxLength; bits++)
            {
                code = (code + blCount[bits - 1]) << 1;
                firstCode[bits] = code;
            }
            return firstCode;
        }

        private static void DecompressStatic(BitReader bitStream, IList<byte> data)
        {
            var decoder = HuffmanTree.StaticTree.GetDecoder();
            while (true)
            {
                var bit = bitStream.ReadBit();
                var value = decoder.Next(bit);
                if (value < 0) continue;
                if (value < 256)
                {
                    data.Add((byte) value);
                    continue;
                }
                if (value == 256) break;
                var length = lengthStart[value - 257] + bitStream.ReadBits(lengthExtraBits[value - 257]);
                var offsetId = bitStream.ReadReversedBits(5);
                var offset = offsetStart[offsetId];
                offset += bitStream.ReadBits(offsetExtraBits[offsetId]);
                for (var i = 0; i < length; i++)
                {
                    data.Add(data[data.Count - offset]);
                }
            }
        }
    }
}