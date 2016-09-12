using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using BrutePack.Deflate;
using JetBrains.Annotations;

namespace BrutePack.GZip
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class GZipDecompressor
    {
        private const int FTEXT = 1;
        private const int FHCRC = 2;
        private const int FEXTRA = 4;
        private const int FNAME = 8;
        private const int FCOMMENT = 16;

        public static void Decompress(Stream input, Stream output)
        {
            var buffer = new byte[10];
            input.Read(buffer, 0, 10);
            if (buffer[0] != 0x1F || buffer[1] != 0x8B)
                throw new FormatException("Invalid gzip header");
            if (buffer[2] != 8)
                throw new NotSupportedException("Compression method is not deflate");
            int FLG = buffer[3];
            if (FLG != FNAME && FLG != 0)
                throw new NotSupportedException("Only FNAME flag is supported");
            // 4-7: MTIME
            int XFL = buffer[8];
            if (XFL == 2)
                Console.Out.WriteLine("Extra flag: best compression algorithm (XFL = 2)");
            if (XFL == 4)
                Console.Out.WriteLine("Extra flag: fastest compression algorithm (XFL = 4)");
            // 9: OS
            if ((FLG & FNAME) != 0)
            {
                buffer = new byte[256];
                var offset = 0;
                do
                {
                    input.Read(buffer, offset, 1);
                    offset++;
                } while (buffer[offset - 1] != '\0');
                var name = System.Text.Encoding.ASCII.GetString(buffer, 0, offset);
                Console.WriteLine("filename=" + name);
            }
            DeflateDecompressor.Decompress(input, output);
        }
    }
}