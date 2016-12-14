using System;
using System.IO;
using System.Numerics;
using System.Xml.Schema;
//using Gnu.MP;
using BrutePack.Gmp;

namespace BrutePack.ArithmeticCoding
{
    public static class ArithmeticCoder
    {
        private static Rational[] BuildFrequencyTable(int[] frequencies, int totalSize)
        {
            var frequencyPoints = new Rational[257];
            frequencyPoints[0] = new Rational(0d);
            for (int i = 1; i < 257; i++)
                frequencyPoints[i] = frequencyPoints[i - 1] + new Rational(frequencies[i - 1], totalSize);
            return frequencyPoints;
        }

        public static int[] CalculateFrequencies(byte[] data)
        {
            return CalculateFrequencies(data, data.Length);
        }

        public static int[] CalculateFrequencies(byte[] data, int dataSize)
        {
            var result = new int[256];
            for (int i = 0; i < dataSize; i++)
            {
                result[data[i]]++;
            }
            return result;
        }

        public static byte[] Encode(byte[] data, int[] frequencies)
        {
            return Encode(data, frequencies, data.Length);
        }

        public static byte[] Encode(byte[] data, int[] frequencies, int dataLength, int innerChunkSize = 128)
        {
            var leftBound = new Rational(0d);
            var rightBound = new Rational(256, 1);
            var frequencyPoints = BuildFrequencyTable(frequencies, dataLength);
            var outStream = new MemoryStream();
            var tempOut = new MemoryStream(128);
            var outCount = 0;

            for (int i = 0; i < dataLength; i++)
            {
                rightBound.SubAssign(leftBound); // rightBound is now span
                var newRightBound = leftBound + rightBound * frequencyPoints[data[i] + 1];
                rightBound.MulAssign(frequencyPoints[data[i]]);
                leftBound.PlusAssign(rightBound);
                rightBound = newRightBound;
                var leftParts = leftBound.GetWholeAndFraction();
                var rightParts = rightBound.GetWholeAndFraction();
                if (leftParts.Whole == rightParts.Whole)
                {
                    tempOut.WriteByte((byte) rightParts.Whole);
                    leftBound = leftParts.Fraction;
                    rightBound = rightParts.Fraction;
                    leftBound.MulAssign(256);
                    rightBound.MulAssign(256);
                }
                outCount++;
                if (outCount == innerChunkSize - 1)
                {
                    while (true)
                    {
                        leftParts = leftBound.GetWholeAndFraction();
                        rightParts = rightBound.GetWholeAndFraction();
                        tempOut.WriteByte((byte) rightParts.Whole);
                        if (leftParts.Whole != rightParts.Whole)
                            break;
                        leftBound = leftParts.Fraction;
                        rightBound = rightParts.Fraction;
                        leftBound.MulAssign(256);
                        rightBound.MulAssign(256);
                    }
                    leftBound.MulAssign(ZERO);
                    rightBound = new Rational(256, 1);
                    WriteVarInt(outStream, (int) tempOut.Length);
                    WriteVarInt(outStream, outCount);
                    outStream.Write(tempOut.GetBuffer(), 0, (int) tempOut.Length);
                    tempOut.Position = 0;
                    tempOut.SetLength(0);
                    outCount = 0;
                }
            }

            while (true)
            {
                var leftParts = leftBound.GetWholeAndFraction();
                var rightParts = rightBound.GetWholeAndFraction();
                tempOut.WriteByte((byte) rightParts.Whole);
                if (leftParts.Whole != rightParts.Whole)
                    break;
                leftBound = leftParts.Fraction;
                rightBound = rightParts.Fraction;
                leftBound.MulAssign(256);
                rightBound.MulAssign(256);

            }
            WriteVarInt(outStream, (int) tempOut.Length);
            WriteVarInt(outStream, outCount);
            outStream.Write(tempOut.GetBuffer(), 0, (int) tempOut.Length);

            return outStream.ToArray();
        }

        private static readonly Rational ZERO = new Rational(0);

        public static byte[] Decode(byte[] data, int[] frequencies, int outputSize)
        {
            var leftBound = new Rational(0d);
            var rightBound = new Rational(1, 1);
            var inStream = new MemoryStream(data);
            byte[] result = new byte[outputSize];
            int i = 0;
            while (inStream.Position < inStream.Length - 1)
            {
                var encodedSubSize = ReadVarInt(inStream);
                var decodedNumBytes = ReadVarInt(inStream);

                var frequencyPoints = BuildFrequencyTable(frequencies, outputSize);
                var point = new Rational(0d);
                for (int j = (int) (inStream.Position + encodedSubSize - 1); j >= inStream.Position; j--)
                    point = (point + data[j]) / 256;
                inStream.Seek(encodedSubSize, SeekOrigin.Current);

                for (; decodedNumBytes > 0; i++, decodedNumBytes--)
                {
                    var scaledPoint = (point - leftBound) / (rightBound - leftBound);
                    for (int j = 255; j >= 0; j--)
                    {
                        if (scaledPoint >= frequencyPoints[j])
                        {
                            var span = (rightBound - leftBound);
                            rightBound = leftBound + span * frequencyPoints[j + 1];
                            leftBound = leftBound + span * frequencyPoints[j];
                            result[i] = (byte) j;
                            break;
                        }
                    }
                }
            }

            return result;
        }

        private static void WriteVarInt(Stream s, int value)
        {
            if(value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "value must be greater than zero");
            while (true)
            {
                if (value < 127)
                {
                    s.WriteByte((byte) value);
                    return;
                }
                s.WriteByte((byte) (0x80 | value & 0x7f));
                value >>= 7;
            }
        }

        private static int ReadVarInt(Stream s)
        {
            var result = 0;
            while (true)
            {
                var read = s.ReadByte();
                if(read == -1)
                    throw new EndOfStreamException("Unexpected EOF while reading varint");
                result = (result << 7) | (read & 0x7f);
                if ((read & 0x80) == 0)
                {
                    return result;
                }
            }
        }

        public static void EncodeBlockStream(byte[] data, Stream output, int dataSize, int innerChunkSize = 128)
        {
            var frequencies = CalculateFrequencies(data, dataSize);
            WriteVarInt(output, dataSize);
            for (int i = 0; i < 256; i++)
            {
                WriteVarInt(output, frequencies[i]);
            }
            var encodedData = Encode(data, frequencies, dataSize, innerChunkSize);
            WriteVarInt(output, encodedData.Length);
            output.Write(encodedData, 0, encodedData.Length);
        }

        public static byte[] DecodeBlockStream(Stream input)
        {
            var resultSize = ReadVarInt(input);
            var frequencies = new int[256];
            for (int i = 0; i < 256; i++)
            {
                frequencies[i] = ReadVarInt(input);
            }
            var encodedLength = ReadVarInt(input);
            var encodedData = new byte[encodedLength];
            var currentlyRead = 0;
            while (currentlyRead < encodedLength)
            {
                var read = input.Read(encodedData, currentlyRead, encodedLength - currentlyRead);
                if(read <= 0)
                    throw new EndOfStreamException("Unexpected EOF while reading encoded data");
                currentlyRead += read;
            }
            return Decode(encodedData, frequencies, resultSize);
        }

        public static void EncodeStream(Stream input, Stream output, int blockSize = 65536, int innerChunkSize = 128)
        {
            byte[] block = new byte[blockSize];
            while (true)
            {
                var subBlockRead = 0;
                var isLastBlock = false;
                while (subBlockRead < blockSize)
                {
                    var read = input.Read(block, subBlockRead, blockSize - subBlockRead);
                    if (read == 0)
                    {
                        isLastBlock = true;
                        break;
                    }
                    subBlockRead += read;
                }
                EncodeBlockStream(block, output, subBlockRead, innerChunkSize);
                if (isLastBlock)
                    break;
            }
        }

        public static void DecodeStream(Stream input, Stream output)
        {
            while (input.Position <= input.Length - 1) // todo: better mechanism for EOF detection, reuse block storage memory
            {
                var block = DecodeBlockStream(input);
                output.Write(block, 0, block.Length);
            }
        }
    }
}