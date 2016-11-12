using System.IO;

namespace BrutePack.Compression
{
    public interface ICompressor
    {
        void Compress(Stream inStream, Stream outStream);
    }
}