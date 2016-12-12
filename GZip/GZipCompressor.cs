using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using BrutePack.Compression;
using BrutePack.Deflate;

namespace BrutePack.GZip
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class GZipCompressor
    {
        private const int FTEXT = 1;
        private const int FHCRC = 2;
        private const int FEXTRA = 4;
        private const int FNAME = 8;
        private const int FCOMMENT = 16;

        public static void Compress(Stream inStream, Stream outStream)
        {
            var buffer = new byte[10];
            buffer[0] = 0x1F; // ID1
            buffer[1] = 0x8B; // ID2
            buffer[2] = 8;    // Deflate compression method
            buffer[3] = 0;    // FLG = 0
            buffer[4] = 0;    // MTIME[0] :: MTIME = 0 -- no timestamp available
            buffer[5] = 0;    // MTIME[1]
            buffer[6] = 0;    // MTIME[2]
            buffer[7] = 0;    // MTIME[3]
            buffer[8] = 0;    // no XFLG
            buffer[9] = 11;   // NTFS filesystem
            outStream.Write(buffer, 0, buffer.Length);
//            var crcCount = CopyDeflateCompressor.Compress(inStream, outStream);
            var crcCount = StaticHuffmanDeflateCompressor.Compress(inStream, outStream);
            buffer = new byte[4];
            buffer[0] = (byte) (crcCount.Item1 & 0xFF);
            buffer[1] = (byte) ((crcCount.Item1 >> 8) & 0xFF);
            buffer[2] = (byte) ((crcCount.Item1 >> 16) & 0xFF);
            buffer[3] = (byte) ((crcCount.Item1 >> 24) & 0xFF);
            outStream.Write(buffer, 0, buffer.Length);
            buffer[0] = (byte) (crcCount.Item2 & 0xFF);
            buffer[1] = (byte) ((crcCount.Item2 >> 8) & 0xFF);
            buffer[2] = (byte) ((crcCount.Item2 >> 16) & 0xFF);
            buffer[3] = (byte) ((crcCount.Item2 >> 24) & 0xFF);
            outStream.Write(buffer, 0, buffer.Length);
        }
    }
}