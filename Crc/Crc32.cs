namespace BrutePack.Crc
{
    public class Crc32
    {
        private static readonly uint[] CrcTable = CalcCrcTable();

        private static uint[] CalcCrcTable()
        {
            var crcTable = new uint[256];
            for (uint i = 0; i < 256; i++) {
                var crc = i;
                for (uint j = 0; j < 8; j++)
                    crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320 : crc >> 1;

                crcTable[i] = crc;
            }
            return crcTable;
        }

        public static uint InitCrc() => 0xFFFFFFFF;

        public static void NextCrc(ref uint crc, byte value)
        {
            crc = CrcTable[(crc ^ value) & 0xFF] ^ (crc >> 8);
        }

        public static void FinishCrc(ref uint crc)
        {
            crc ^= 0xFFFFFFFF;
        }
    }
}