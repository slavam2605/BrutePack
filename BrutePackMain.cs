using System;
using System.Diagnostics;
using System.IO;
using BrutePack.GZip;
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

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }

        public static void PrintInt(int x)
        {
            for (var i = 0; i < 32; i++)
            {
                Console.Write(x & 1);
                x >>= 1;
            }
            Console.WriteLine();
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
            try
            {
                var stopwatch = Stopwatch.StartNew();
                if (decompress)
                {
                    GZipDecompressor.Decompress(
                        new FileStream(inputFile, FileMode.Open),
                        new FileStream(outputFile, FileMode.Create)
                    );
                }
                else
                {
                    GZipCompressor.Compress(
                        new FileStream(inputFile, FileMode.Open),
                        new FileStream(outputFile, FileMode.Create)
                    );
                }
                Console.Out.WriteLine("Time elapsed: " + stopwatch.Elapsed);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
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