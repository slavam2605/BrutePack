using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using BrutePack.BitStream;
using BrutePack.GZip;
using BrutePack.Huffman;
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
                if (decompress)
                {
                    var stopwatch = Stopwatch.StartNew();
                    GZipDecompressor.Decompress(
                        new FileStream(inputFile, FileMode.Open),
                        new FileStream(outputFile, FileMode.Create)
                    );
                    Console.Out.WriteLine("Time elapsed: " + stopwatch.Elapsed);
                }
                else
                {
                    GZipCompressor.Compress(
                        new FileStream(inputFile, FileMode.Open),
                        new FileStream(outputFile, FileMode.Create)
                    );
                }
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