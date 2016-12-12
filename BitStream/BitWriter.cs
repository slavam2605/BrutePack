using System;
using System.IO;

namespace BrutePack.BitStream
{
    public class BitWriter : Stream
    {
        private const int MaxCacheSize = 1024;

        private readonly Stream stream;
        private int occupiedBits;
        private byte cachedByte;
        private readonly byte[] cache = new byte[MaxCacheSize];
        private int cacheSize;

        public BitWriter(Stream stream)
        {
            this.stream = stream;
        }

//        public override void stream.WriteByte(byte value)
//        {
//            cache[cacheSize] = value;
//            cacheSize++;
//            if (cacheSize != MaxCacheSize)
//                return;
//            stream.Write(cache, 0, cacheSize);
//            cacheSize = 0;
//        }

        public override void Flush()
        {
            cache[cacheSize] = cachedByte;
            cacheSize++;
            stream.Write(cache, 0, cacheSize);
            cacheSize = 0;
            cachedByte = 0;
            occupiedBits = 0;
            stream.Flush();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (occupiedBits != 0)
                throw new InvalidOperationException("Not on the byte boundary");
            Flush();
            stream.Write(buffer, offset, count);
        }

        public void WriteBits(int value, int count)
        {
            if (count <= 8 - occupiedBits)
            {
                cachedByte |= (byte) (value << occupiedBits);
                occupiedBits += count;
                if (occupiedBits != 8)
                    return;
                stream.WriteByte(cachedByte);
                cachedByte = 0;
                occupiedBits = 0;
            }
            else
            {
                cachedByte |= (byte) (value << occupiedBits); // TODO check (modulo 2^8)
                value >>= 8 - occupiedBits;
                count -= 8 - occupiedBits;
                stream.WriteByte(cachedByte);
                cachedByte = 0;
                occupiedBits = 0;
                while (count >= 8)
                {
                    stream.WriteByte((byte) value);
                    value >>= 8;
                    count -= 8;
                }
                cachedByte += (byte) value;
                occupiedBits = count;

            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new System.NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new System.NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }


        public override long Position { get; set; }
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length { get; }
    }
}