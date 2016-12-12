using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using BrutePack.Decompression;
using BrutePack.ExternalCompressor;

namespace BrutePack.FileFormat
{
    public static class BlockDecompressor
    {
        private static Dictionary<BlockType, IDecompressionProvider> typeToProvider =
            new Dictionary<BlockType, IDecompressionProvider>();

        static BlockDecompressor()
        {
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var type in assembly.DefinedTypes)
            {
                var attrib = type.GetCustomAttribute<DecompressionProviderAttribute>(false);
                if (attrib != null)
                {
                    if (typeToProvider.ContainsKey(attrib.TargetType))
                        throw new ApplicationException(
                            $"Multiple classes containt the DecompressionProvider attribute for {attrib.TargetType}: {typeToProvider[attrib.TargetType].GetType().Name} and {type.Name}");
                    var primaryConstructor = type.GetConstructor(new Type[0]);
                    if (primaryConstructor != null)
                        typeToProvider[attrib.TargetType] =
                            (IDecompressionProvider) primaryConstructor.Invoke(new object[0]);
                    else
                        throw new ApplicationException(
                            $"Class with DecompressionProvider for {attrib.TargetType} ({type.Name}) doesn't have parameterless constructor");
                }
            }
        }

        public static byte[] Decompress(BrutePackBlock block)
        {
            IDecompressionProvider decomp;
            if(!typeToProvider.TryGetValue(block.BlockType, out decomp))
                throw new ApplicationException("Unknown block type: " + block.BlockType);
            return decomp.Decompress(block);
        }
    }
}
