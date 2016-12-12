using System;
using System.Diagnostics;
using System.IO;
using BrutePack.Decompression;
using BrutePack.FileFormat;

namespace BrutePack.ExternalCompressor
{
    [DecompressionProvider(BlockType.External)]
    public class ExternalDecompressionProvider : IDecompressionProvider
    {
        public byte[] Decompress(BrutePackBlock block)
        {
            var memStream = new MemoryStream(block.BlockData);
            var reader = new BinaryReader(memStream);
            var decompressProg = reader.ReadString();

            var split = decompressProg.Split(new[] {' '}, 2);
            var processStart = new ProcessStartInfo(split[0], split.Length > 1 ? split[1] : "")
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
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