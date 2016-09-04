using System.IO;

namespace BrutePack
{
    public interface Compressor
    {
        void Compress(Stream inStream, Stream outStream);
    }
}