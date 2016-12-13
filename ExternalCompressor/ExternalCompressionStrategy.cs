using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using BrutePack.CompressionStrategy;
using BrutePack.FileFormat;

namespace BrutePack.ExternalCompressor
{
    public class ExternalCompressionStrategy : ICompressionStrategy
    {
        public ExternalCompressorConfig Config { get; }

        public ExternalCompressionStrategy(ExternalCompressorConfig config)
        {
            this.Config = config;
        }

        public BrutePackBlock? CompressBlock(byte[] data, int length)
        {
            MemoryStream memStream = new MemoryStream();
            var writer = new BinaryWriter(memStream);
            writer.Write(Config.UncompressCommand);

            var split = Config.CompressCommand.Split(new[] {' '}, 2);
            var processStart = new ProcessStartInfo(split[0], split.Length > 1 ? split[1] : "");
            processStart.RedirectStandardInput = true;
            processStart.RedirectStandardOutput = true;
            processStart.UseShellExecute = false;
            processStart.CreateNoWindow = true;
            var cproc = Process.Start(processStart);
            if (cproc == null)
                throw new InvalidProgramException();

            var readTask = cproc.StandardInput.BaseStream.WriteAsync(data, 0, length);
            readTask.ContinueWith(_ => cproc.StandardInput.BaseStream.Close());
            var writeTask = CopyStreamTo(cproc.StandardOutput.BaseStream, memStream);

            /*var sched = TaskScheduler.Default;
            readTask.Start(sched);
            writeTask.Start(sched);*/

            readTask.Wait();
            writeTask.Wait();

            if (memStream.Position > 65535)
                return null;
            return new BrutePackBlock(BlockType.External, memStream.ToArray());
        }

        public static async Task CopyStreamTo(Stream a, Stream b)
        {
            while (true)
            {
                var buf = new byte[65536];
                var rdSize = await a.ReadAsync(buf, 0, buf.Length);
                if (rdSize == 0)
                    return;
                await b.WriteAsync(buf, 0, rdSize);
            }
        }
    }
}