namespace BrutePack.Util
{
    public static class StringEx
    {
        public static bool NullOrEmpty(this string str)
        {
            return str == null || str.Trim() == "";
        }
    }
}