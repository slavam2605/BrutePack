using System;
using System.IO;
using System.Numerics;
using System.Xml.Schema;
using Gnu.MP;

namespace BrutePack.ArithmeticCoding
{
    public static class ArithmeticCoder
    {
        public static Rational GetWholePart(this Rational num)
        {
            return num - num.GetFractionPart();
        }

        public static Rational GetFractionPart(this Rational num)
        {
            return new Rational(num.Numerator % num.Denumerator, num.Denumerator);
        }

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

        public static byte[] Encode(byte[] data, int[] frequencies, int dataLength)
        {
            var leftBound2 = new Rational(0);
            var leftBound = new Rational(0d);
            var rightBound = new Rational(256, 1);
            var frequencyPoints = BuildFrequencyTable(frequencies, dataLength);
            var outStream = new MemoryStream();

            for (int i = 0; i < dataLength; i++)
            {
                var span = (rightBound - leftBound);
                rightBound = leftBound + span * frequencyPoints[data[i] + 1];
                leftBound = leftBound + span * frequencyPoints[data[i]];
                if (leftBound.GetWholePart() == rightBound.GetWholePart())
                {
                    outStream.WriteByte((byte) rightBound.GetWholePart());
                    leftBound = leftBound.GetFractionPart();
                    rightBound = rightBound.GetFractionPart();
                    leftBound *= 256;
                    rightBound *= 256;
                }
            }

            while (true)
            {
                outStream.WriteByte((byte) rightBound.GetWholePart());
                if (leftBound.GetWholePart() != rightBound.GetWholePart())
                    return outStream.ToArray();
                leftBound = leftBound.GetFractionPart();
                rightBound = rightBound.GetFractionPart();
                leftBound *= 256;
                rightBound *= 256;
            }
        }

        public static byte[] Decode(byte[] data, int[] frequencies, int outputSize)
        {
            var leftBound = new Rational(0d);
            var rightBound = new Rational(1, 1);
            var frequencyPoints = BuildFrequencyTable(frequencies, outputSize);
            var point = new Rational(0d);
            for (int i = data.Length - 1; i >= 0; i--)
                point = (point + data[i]) / 256;

            byte[] result = new byte[outputSize];
            for (int i = 0; i < outputSize; i++)
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

        public static void EncodeBlockStream(byte[] data, Stream output, int dataSize)
        {
            var frequencies = CalculateFrequencies(data, dataSize);
            WriteVarInt(output, dataSize);
            for (int i = 0; i < 256; i++)
            {
                WriteVarInt(output, frequencies[i]);
            }
            var encodedData = Encode(data, frequencies, dataSize);
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

        public static void EncodeStream(Stream input, Stream output, int blockSize = 65536)
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
                EncodeBlockStream(block, output, subBlockRead);
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