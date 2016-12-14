using System;
using BrutePack.FileFormat;
using JetBrains.Annotations;

namespace BrutePack.Decompression
{
    [MeansImplicitUse]
    public class DecompressionProviderAttribute : Attribute
    {
        public BlockType TargetType { get; private set; }

        public DecompressionProviderAttribute(BlockType targetType)
        {
            this.TargetType = targetType;
        }
    }
}