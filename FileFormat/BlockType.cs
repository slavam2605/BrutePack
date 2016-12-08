namespace BrutePack.FileFormat
{
    public enum BlockType: byte
    {
        Uncompressed = 0,
        GZip = 1,
        External = 255,
    }
}