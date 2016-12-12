namespace BrutePack.LempelZiv
{
    public class LZ77
    {
        public static short[] Compress(byte[] data, int size)
        {
            var result = new short[size];
            for (var i = 0; i < size; i++)
            {
                result[i] = data[i];
            }
            return result;
        }
    }
}