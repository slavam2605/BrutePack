using System;
using System.IO;

namespace BrutePack.FileFormat
{
    public struct BrutePackBlock
    {
        public BrutePackBlock(BlockType blockType, byte[] blockData)
        {
            if (blockData.Length > 65536)
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
            var blockSize = reader.ReadUInt16();
            var blockData = reader.ReadBytes(blockSize);
            return new BrutePackBlock((BlockType) blockType, blockData);
        }

        public static void WriteBrutePackBlock(this BinaryWriter writer, BrutePackBlock block)
        {
            writer.Write((byte) block.BlockType);
            writer.Write((ushort) block.BlockData.Length);
            writer.Write(block.BlockData);
        }
    }
}
