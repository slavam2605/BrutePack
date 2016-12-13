using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BrutePack.CompressionStrategy;
using BrutePack.FileFormat;
using BrutePack.GZip;
using BrutePack.StrategyConfig;
using BrutePack.Util;
using CommandLine;
using CommandLine.Text;

namespace BrutePack
{
    public class BrutePackMain
    {
        [Option('i', "input", Required = true, HelpText = "Input file")]
        public string InputFile { get; set; }

        [Option('o', "output", DefaultValue = "", HelpText = "Output file")]
        public string OutputFile { get; set; }

        [Option('d', "decompress", DefaultValue = false, HelpText = "Decompress mode")]
        public bool Decompress { get; set; }

        [Option('c', "config", MutuallyExclusiveSet = "f", HelpText = "Compression configuration string")]
        public string ConfigString { get; set; }

        [Option('f', "file", MutuallyExclusiveSet = "c", HelpText = "Compression configuration file")]
        public string ConfigFile { get; set; }

        [Option('b', "max-buffer", DefaultValue = 65000, HelpText = "Maximum block size for compression (1-65535)")]
        public int MaxBufferSize { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }

        public static void Main(string[] args)
        {
            var brutePackMain = new BrutePackMain();
            if (!Parser.Default.ParseArguments(args, brutePackMain)) return;
            var inputFile = brutePackMain.InputFile;
            var outputFile = brutePackMain.OutputFile;
            var decompress = brutePackMain.Decompress;

            if (outputFile == "")
            {
                GenerateOutputFilename(inputFile, decompress, out outputFile);
            }

            if (!decompress)
            {
                if (brutePackMain.ConfigFile.NullOrEmpty() && brutePackMain.ConfigString.NullOrEmpty())
                {
                    Console.WriteLine("You must specify -c or -f for compression");
                    return;
                }
            }

            var inputStream = new FileStream(inputFile, FileMode.Open);
            var outputStream = new FileStream(outputFile, FileMode.CreateNew);


            if (decompress)
            {
                using(var uncompressStream = new BruteUncompressingStream(new BinaryReader(inputStream)))
                    uncompressStream.CopyTo(outputStream);
                outputStream.Flush();
                outputStream.Close();
            }
            else
            {
                var configString = brutePackMain.ConfigFile.NullOrEmpty()
                    ? brutePackMain.ConfigString
                    : File.ReadAllText(brutePackMain.ConfigFile);
                var parsedConfig =
                    StrategyConfigParser.ParseConfig(configString)
                        .Concat(Enumerable.Repeat<ICompressionStrategy>(new DumbCompressionStrategy(), 1));
                var compressingStream = new BruteCompressingStream(new BinaryWriter(outputStream),
                    brutePackMain.MaxBufferSize, new BruteCompressionStrategy(parsedConfig));
                inputStream.CopyTo(compressingStream);
                compressingStream.Flush();
                compressingStream.Close();
            }
        }

        private static void GenerateOutputFilename(string inputFile, bool decompress, out string outputFile)
        {
            if (decompress)
            {
                var dotPos = inputFile.LastIndexOf('.');
                if (dotPos >= 0)
                {
                    outputFile = inputFile.Substring(0, dotPos);
                }
                else
                {
                    outputFile = inputFile + ".o";
                }
            }
            else
            {
                outputFile = inputFile + ".gz";
            }
        }
    }
}