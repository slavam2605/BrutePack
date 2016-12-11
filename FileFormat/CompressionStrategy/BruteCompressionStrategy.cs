using System.Collections.Generic;

namespace BrutePack.FileFormat.CompressionStrategy
{
    public class BruteCompressionStrategy : ICompressionStrategy
    {
        private readonly List<ICompressionStrategy> subStrategies;

        public BruteCompressionStrategy(List<ICompressionStrategy> subStrategies)
        {
            this.subStrategies = subStrategies;
        }

        public BrutePackBlock? CompressBlock(byte[] data, int length)
        {
            int minSize = int.MaxValue;
            BrutePackBlock? bestBlock = null;
            foreach (var strategy in subStrategies)
            {
                var compressed = strategy.CompressBlock(data, length);
                if (compressed != null && compressed.Value.BlockData.Length < minSize)
                {
                    bestBlock = compressed;
                }
            }
            return bestBlock;
        }
    }
}