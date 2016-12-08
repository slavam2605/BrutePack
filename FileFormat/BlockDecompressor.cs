using System;

namespace BrutePack.FileFormat
{
    public class BlockDecompressor
    {
        public static byte[] Decompress(BrutePackBlock block)
        {
            switch (block.BlockType)
            {
                case BlockType.Uncompressed:
                    return block.BlockData;
                default:
                    throw new NotImplementedException("Unknown block type " + block.BlockType + " not supported");
            }
        }
    }
}
