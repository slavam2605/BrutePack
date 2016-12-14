using System.Collections.Generic;
using BrutePack.ArithmeticCoding;
using BrutePack.CompressionStrategy;
using BrutePack.ExternalCompressor;
using BrutePack.GZip;
using CommandLine;

namespace BrutePack.StrategyConfig
{
    public static class StrategyConfigParser
    {
        public static IEnumerable<ICompressionStrategy> ParseConfig(string config)
        {
            var split = config.Split(';');
            foreach (var s in split)
            {
                var innerSplit = s.Split(',');
                var firstPart = innerSplit[0].Trim();
                switch (firstPart)
                {
                    case "":
                        break;
                    case "gzip":
                        yield return new GZipCompressionStrategy();
                        break;
                    case "arithm":
                        yield return new ArithmeticCodingStrategy(innerSplit.Length > 1 ? int.Parse(innerSplit[1]) : 64);
                        break;
                    case "ext":
                        yield return new ExternalCompressionStrategy(new ExternalCompressorConfig(innerSplit[1], innerSplit[2]));
                        break;
                    default:
                        throw new ParserException("Unknown config parameter " + firstPart);
                }
            }
        }
    }
}