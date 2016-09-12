using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Threading;
using BitStream;
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

        private static int ReadBits(BitStream.BitStream stream, int bits)
        {
            var currentPower = 1;
            var value = 0;
            for (var i = 0; i < bits; i++)
            {
                byte bit;
                stream.ReadBits(out bit, (BitNum) 1);
                value += currentPower * bit;
                currentPower <<= 1;
            }
            return value;
        }

        private static int ReadReversedBits(BitStream.BitStream stream, int bits)
        {
            if (bits <= 8)
            {
                byte bvalue;
                stream.ReadBits(out bvalue, (BitNum) bits);
                return bvalue;
            }
            var value = 0;
            for (var i = 0; i < bits; i++)
            {
                byte bit;
                stream.ReadBits(out bit, (BitNum) 1);
                value <<= 1;
                value += bit;
            }
            return value;
        }

        private static int FetchValue(BitStream.BitStream stream, HuffmanTree.HuffmanDecoder decoder)
        {
            while (true)
            {
                byte bit;
                stream.ReadBits(out bit, (BitNum) 1);
                var value = decoder.Next(bit);
                if (value >= 0)
                    return value;
            }
        }

        private static void FinishByte(BitStream.BitStream stream)
        {
            byte rest;
            stream.ReadBits(out rest, (BitNum) (8 - stream.BitPosition));
        }

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

        private static void DecompressCopy(BitStream.BitStream bitStream, IList<byte> data)
        {
            FinishByte(bitStream);

        }

        private static void DecompressDynamic(BitStream.BitStream bitStream, IList<byte> data)
        {
//            while (true)
//            {
//                byte bit;
//                if (!bitStream.ReadBits(out bit, (BitNum) 1))
//                    break;
//                Console.Write(bit);
//            }
//            return;

            var lengthTree = new HuffmanTree();
            var literalTree = new HuffmanTree();
            var offsetTree = new HuffmanTree();

            var hlit = 257 + ReadBits(bitStream, 5);
            var hdist = 1 + ReadBits(bitStream, 5);
            var hclen = 4 + ReadBits(bitStream, 4);

            Console.WriteLine(hlit + ", " + hdist + ", " + hclen);

            var lengthLengths = new int[19];
            for (var i = 0; i < hclen; i++)
            {
                lengthLengths[lengthsOrder[i]] = ReadBits(bitStream, 3);
            }
            GenCodes(lengthLengths, lengthTree);

            var literalLengths = new int[287];
            ReadLengths(bitStream, lengthTree, literalLengths, hlit);
            GenCodes(literalLengths, literalTree);

            var offsetLengths = new int[32];
            ReadLengths(bitStream, lengthTree, offsetLengths, hdist);
            GenCodes(offsetLengths, offsetTree);

            Console.WriteLine(literalTree.ToString());
            Console.WriteLine(offsetTree.ToString());

            var literalDecoder = literalTree.GetDecoder();
            var offsetDecoder = offsetTree.GetDecoder();
            while (true)
            {
                byte bit;
                if (!bitStream.ReadBits(out bit, (BitNum) 1))
                    break;
                var value = literalDecoder.Next(bit);
                if (value < 0) continue;
//                Console.WriteLine(value);
                if (value < 256)
                {
                    data.Add((byte) value);
                    continue;
                }
                if (value == 256) break;
                var length = lengthStart[value - 257] + ReadBits(bitStream, lengthExtraBits[value - 257]);
                var offsetId = FetchValue(bitStream, offsetDecoder);
                var offset = offsetStart[offsetId];
                offset += ReadBits(bitStream, offsetExtraBits[offsetId]);
//                Console.WriteLine("<" + length + ", " + offset + ">");
                for (var i = 0; i < length; i++)
                {
                    data.Add(data[data.Count - offset]);
                }
            }
        }

        private static void ReadLengths(BitStream.BitStream stream, HuffmanTree tree, int[] lengths, int count)
        {
            var lengthsDecoder = tree.GetDecoder();
            var readLiterals = 0;
            while (readLiterals < count)
            {
                byte bit;
                stream.ReadBits(out bit, (BitNum) 1);
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
                        var extraBits = 3 + ReadBits(stream, 2);
                        for (var i = 0; i < extraBits; i++)
                        {
                            lengths[readLiterals + i] = lengths[readLiterals - 1];
                        }
                        readLiterals += extraBits;
                        break;
                    }
                    case 17:
                    {
                        var extraBits = ReadBits(stream, 3);
                        readLiterals += 3 + extraBits;
                        break;
                    }
                    case 18:
                    {
                        var extraBits = ReadBits(stream, 7);
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

        private static void DecompressStatic(BitStream.BitStream bitStream, IList<byte> data)
        {
            var decoder = HuffmanTree.StaticTree.GetDecoder();
            while (true)
            {
                byte bit;
                if (!bitStream.ReadBits(out bit, (BitNum) 1))
                    break;
                var value = decoder.Next(bit);
                if (value < 0) continue;
//                Console.WriteLine(value);
                if (value < 256)
                {
                    data.Add((byte) value);
                    continue;
                }
                if (value == 256) break;
                var length = lengthStart[value - 257] + ReadBits(bitStream, lengthExtraBits[value - 257]);
                var offsetId = ReadReversedBits(bitStream, 5);
                var offset = offsetStart[offsetId];
                offset += ReadBits(bitStream, offsetExtraBits[offsetId]);
//                Console.WriteLine("<" + length + ", " + offset + ">");
                for (var i = 0; i < length; i++)
                {
                    data.Add(data[data.Count - offset]);
                }
            }
        }
    }
}