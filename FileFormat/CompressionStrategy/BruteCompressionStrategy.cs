using System.Collections.Generic;
using System.Linq;

namespace BrutePack.FileFormat.CompressionStrategy
{
    public class BruteCompressionStrategy : ICompressionStrategy
    {
        private readonly List<ICompressionStrategy> subStrategies;

        public BruteCompressionStrategy(IEnumerable<ICompressionStrategy> subStrategies)
        {
            this.subStrategies = subStrategies.ToList();
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
                    minSize = compressed.Value.BlockData.Length;
                }
            }
            return bestBlock;
        }
    }
}
