using System.IO;

namespace BrutePack.Compression
{
    public class DummyCompressor : Compressor
    {
        private const int CHUNK_SIZE = 32768;

        public void Compress(Stream input, Stream output)
        {
            var reader = new BinaryReader(input);
            var writer = new BinaryWriter(output);
            byte[] buffer;
            while ((buffer = reader.ReadBytes(CHUNK_SIZE)).Length > 0)
            {
                writer.Write(buffer);
            }
            writer.Flush();
        }
    }
}