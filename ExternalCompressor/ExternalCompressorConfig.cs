namespace BrutePack.ExternalCompressor
{
    public class ExternalCompressorConfig
    {
        public ExternalCompressorConfig(string compressCommand, string uncompressCommand)
        {
            CompressCommand = compressCommand;
            UncompressCommand = uncompressCommand;
        }

        public string CompressCommand { get; private set; }
        public string UncompressCommand { get; private set; }
    }
}