using System.IO;

namespace BrutePack.Compression
{
    public class DummyCompressor : ICompressor
    {
        private const int ChunkSize = 32768;

        public void Compress(Stream input, Stream output)
        {
            var reader = new BinaryReader(input);
            var writer = new BinaryWriter(output);
            byte[] buffer;
            while ((buffer = reader.ReadBytes(ChunkSize)).Length > 0)
            {
                writer.Write(buffer);
            }
            writer.Flush();
        }
    }
}