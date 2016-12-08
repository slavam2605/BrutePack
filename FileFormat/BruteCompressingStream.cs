using System;
using System.IO;

namespace BrutePack.FileFormat
{
    public class BruteCompressingStream : Stream
    {
        private readonly BinaryWriter internalWriter;
        private readonly byte[] internalBuffer;
        private int bufferOffset;

        public BruteCompressingStream(BinaryWriter internalWriter, int maxBufferSize)
        {
            this.internalWriter = internalWriter;
            internalBuffer = new byte[maxBufferSize];
            bufferOffset = 0;
        }

        public override void Flush()
        {
            FlushInternal(true);
        }

        private void FlushInternal(bool flushUnderlyingStream)
        {
            if (bufferOffset > 0)
            {
                var compressedBlock = BlockCompressor.CompressBlock(internalBuffer, bufferOffset);
                internalWriter.WriteBrutePackBlock(compressedBlock);
                bufferOffset = 0;
            }
            if (flushUnderlyingStream)
                internalWriter.Flush();
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

        public override void Write(byte[] buffer, int offset, int count)
        {
            while (count > 0)
            {
                var copySize = Math.Min(count, internalBuffer.Length - bufferOffset);
                Array.Copy(buffer, offset, internalBuffer, bufferOffset, copySize);
                count -= copySize;
                offset += copySize;
                if (bufferOffset == internalBuffer.Length)
                    FlushInternal(false);
            }
        }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => -1;
        public override long Position { get; set; }
    }
}
