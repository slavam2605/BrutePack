using System;
using System.IO;
using JetBrains.Annotations;

namespace BrutePack.BitStream
{
    public class BitReader : Stream
    {
        private readonly Stream stream;
        private int avaliableBits = 0;
        private byte cachedByte;

        public BitReader(Stream stream)
        {
            this.stream = stream;
        }

        private void LoadByte()
        {
            cachedByte = (byte) stream.ReadByte();
            avaliableBits = 8;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (avaliableBits != 0)
                throw new ApplicationException("Unaligned read");
            return stream.Read(buffer, offset, count);
        }

        public byte ReadBit()
        {
            if (avaliableBits == 0)
                LoadByte();
            var result = (byte) (cachedByte & 1);
            cachedByte >>= 1;
            avaliableBits--;
            return result;
        }

        public int ReadBits(int count)
        {
            if (count >= avaliableBits)
            {
                var result = (int) cachedByte;
                var currentPower = 1 << avaliableBits;
                count -= avaliableBits;
                FinishByte();

                while (count >= 8)
                {
                    result += currentPower * stream.ReadByte();
                    currentPower <<= 8;
                    count -= 8;
                }

                if (count <= 0) return result;
                LoadByte();
                result += currentPower * (cachedByte & ((1 << count) - 1));
                cachedByte >>= count;
                avaliableBits -= count;
                return result;
            }
            else
            {
                var result = cachedByte & ((1 << count) - 1);
                cachedByte >>= count;
                avaliableBits -= count;
                return result;
            }
        }

        public int ReadReversedBits(int count)
        {
            var result = 0;
            while (count > 0)
            {
                result <<= 1;
                result += ReadBit();
                count--;
            }
            return result;
        }

        public void FinishByte()
        {
            avaliableBits = 0;
            cachedByte = 0;
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }
}