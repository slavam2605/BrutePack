using System.IO;

namespace BrutePack.Compression
{
    public interface Compressor
    {
        void Compress(Stream inStream, Stream outStream);
    }
}