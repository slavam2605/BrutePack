using System;
using BrutePack.FileFormat;

namespace BrutePack.Decompression
{
    public class DecompressionProviderAttribute : Attribute
    {
        public BlockType TargetType { get; private set; }

        public DecompressionProviderAttribute(BlockType targetType)
        {
            this.TargetType = targetType;
        }
    }
}