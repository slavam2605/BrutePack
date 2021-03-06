﻿using System;
using System.IO;
using BrutePack.Crc;

namespace BrutePack.Deflate
{
    public class CopyDeflateCompressor
    {
        public static Tuple<uint, uint> Compress(Stream inStream, Stream outStream)
        {
            var crc = Crc32.InitCrc();
            var count = 0u;
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
                var tempBuffer = new byte[5];
                tempBuffer[0] = (byte) (size < (1 << 16) - 1 ? 1 : 0);
                tempBuffer[1] = (byte) (size & 0xFF);
                tempBuffer[2] = (byte) (size >> 8);
                tempBuffer[3] = (byte) ~tempBuffer[1];
                tempBuffer[4] = (byte) ~tempBuffer[2];
                outStream.Write(tempBuffer, 0, tempBuffer.Length);
                outStream.Write(buffer, 0, size);
            }
            Crc32.FinishCrc(ref crc);
            return new Tuple<uint, uint>(crc, count);
        }
    }
}