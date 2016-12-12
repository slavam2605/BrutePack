using System;
using System.IO;
using BrutePack.Crc;
using BrutePack.LempelZiv;

namespace BrutePack.Deflate
{
    public class StaticHuffmanDeflateCompressor
    {
        public static Tuple<uint, uint> Compress(Stream inStream, Stream outStream)
        {
            var crc = Crc32.InitCrc();
            var count = 0u;
            var bitOut = new BitStream.BitWriter(outStream);
            var lookupPair = Huffman.HuffmanTree.StaticTree.GetLookupTable(287);
            var codeTable = lookupPair.Item1;
            var lengthTable = lookupPair.Item2;
            while (true)
            {
                var buffer = new byte[(1 << 16) - 1];
                var size = 0;
                int delta;
                while ((delta = inStream.Read(buffer, size, (1 << 16) - 1 - size)) > 0)
                {
                    size += delta;
                }
                if (size == 0)
                    break;
                count += (uint) size; // TODO is it guarantees to be correct after overflow (modulo 1 << 32)?
                for (var i = 0; i < size; i++)
                {
                    Crc32.NextCrc(ref crc, buffer[i]);
                }
                var lzCompressed = LZ77.Compress(buffer, size);
                bitOut.WriteBits(2, 3);
                for (var i = 0; i < size; i++)
                {
                    bitOut.WriteBits(codeTable[lzCompressed[i]], lengthTable[lzCompressed[i]]);
                }
                bitOut.WriteBits(codeTable[256], lengthTable[256]);
            }
            bitOut.WriteBits(3, 3);
            bitOut.WriteBits(codeTable[256], lengthTable[256]);
            bitOut.Flush();
            Crc32.FinishCrc(ref crc);
            return new Tuple<uint, uint>(crc, count);
        }
    }
}