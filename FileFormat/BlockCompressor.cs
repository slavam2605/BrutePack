﻿using System;

namespace BrutePack.FileFormat
{
    public static class BlockCompressor
    {
        public static BrutePackBlock CompressBlock(byte[] data, int length)
        {
            if(length > 65535 || length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            byte[] newData = new byte[length];
            Array.Copy(data, newData, length);
            return new BrutePackBlock(BlockType.Uncompressed, newData);
        }
    }
}
