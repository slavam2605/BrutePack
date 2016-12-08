using System;
using System.IO;
using JetBrains.Annotations;

namespace BrutePack.FileFormat
{
    public class BruteUncompressingStream : Stream
    {
        private readonly BinaryReader internalReader;
        private byte[] remainingBytes;
        private int remainingBytesOffset;

        public BruteUncompressingStream([NotNull] BinaryReader internalReader)
        {
            this.internalReader = internalReader;
            remainingBytes = new byte[0];
            remainingBytesOffset = 0;
        }

        public override void Flush()
        {
            throw new System.NotImplementedException();
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
            var bytesWritten = 0;
            while (count > 0)
            {
                var remainingLength = remainingBytes.Length - remainingBytesOffset;
                if (remainingLength >= count)
                {
                    Array.Copy(remainingBytes, remainingBytesOffset, buffer, offset, count);
                    remainingBytesOffset += count;
                    bytesWritten += count;
                    count -= count;
                }
                else
                {
                    Array.Copy(remainingBytes, remainingBytesOffset, buffer, offset, remainingLength);
                    bytesWritten += remainingLength;
                    count -= remainingLength;
                    offset += remainingLength;
                    remainingBytesOffset += remainingLength;
                    if (!ReadNextBlock())
                        return bytesWritten;
                }
            }
            return bytesWritten;
        }

        private bool ReadNextBlock()
        {
            if(remainingBytesOffset != remainingBytes.Length) // todo: replace with proper assert
                throw new ApplicationException("Assertion failed: remainingBytesOffset != remainingBytes.Length");

            if (internalReader.BaseStream.Position == internalReader.BaseStream.Length)
                return false; // todo: more proper eof detection?

            var block = internalReader.ReadBlock();
            remainingBytes = BlockDecompressor.Decompress(block);
            remainingBytesOffset = 0;
            return true;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => -1;
        public override long Position { get; set; }
    }
}
