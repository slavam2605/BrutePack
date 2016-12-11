using System;
using System.Diagnostics;
using System.IO;
using BrutePack.ExternalCompressor;

namespace BrutePack.FileFormat
{
    public class BlockDecompressor
    {
        public static byte[] Decompress(BrutePackBlock block)
        {
            switch (block.BlockType)
            {
                case BlockType.Uncompressed:
                    return block.BlockData;
                case BlockType.External:
                    return DecompressExternalBlock(block);
                default:
                    throw new NotImplementedException("Unknown block type " + block.BlockType + " not supported");
            }
        }

        private static byte[] DecompressExternalBlock(BrutePackBlock block)
        {
            var memStream = new MemoryStream(block.BlockData);
            var reader = new BinaryReader(memStream);
            var decompressProg = reader.ReadString();

            var split = decompressProg.Split(new[] {' '}, 2);
            var processStart = new ProcessStartInfo(split[0], split.Length > 1 ? split[1] : "");
            processStart.RedirectStandardInput = true;
            processStart.RedirectStandardOutput = true;
            processStart.UseShellExecute = false;
            var cproc = Process.Start(processStart);
            if (cproc == null)
                throw new InvalidProgramException();

            var writeTask = cproc.StandardInput.BaseStream.WriteAsync(block.BlockData, (int) memStream.Position, block.BlockData.Length - (int) memStream.Position);
            writeTask.ContinueWith(_ => cproc.StandardInput.BaseStream.Close());

            var memOutput = new MemoryStream();
            var readTask = ExternalCompressionStrategy.CopyStreamTo(cproc.StandardOutput.BaseStream, memOutput);

            writeTask.Wait();
            readTask.Wait();

            return memOutput.ToArray();
        }
    }
}
