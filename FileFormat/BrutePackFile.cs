using System;
using System.IO;

namespace BrutePack.FileFormat
{
    public struct BrutePackBlock
    {
        public BrutePackBlock(BlockType blockType, byte[] blockData)
        {
            if (blockData.Length >= 1024*1024*2)
                throw new ArgumentOutOfRangeException(nameof(blockData));
            BlockType = blockType;
            BlockData = blockData;
        }

        public BlockType BlockType { get; private set; }
        public byte[] BlockData { get; private set; }
    }

    public static class BrutePackFileEx
    {
        public static BrutePackBlock ReadBlock(this BinaryReader reader)
        {
            var blockType = reader.ReadByte();
            var blockSize = (int) reader.ReadUInt16();
            var blockSubSize = reader.ReadByte();
            blockSize |= (((int) blockSubSize) & 0xff) << 16;
            var blockData = reader.ReadBytes(blockSize);
            return new BrutePackBlock((BlockType) blockType, blockData);
        }

        public static void WriteBrutePackBlock(this BinaryWriter writer, BrutePackBlock block)
        {
            writer.Write((byte) block.BlockType);
            writer.Write((ushort) (block.BlockData.Length & 0xffff));
            writer.Write((byte) (block.BlockData.Length >> 16));
            writer.Write(block.BlockData);
        }
    }
}
