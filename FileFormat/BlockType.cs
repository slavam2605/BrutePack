namespace BrutePack.FileFormat
{
    public enum BlockType: byte
    {
        Uncompressed = 0,
        GZip = 1,
        Arithmetic = 2,
        External = 255,
    }
}