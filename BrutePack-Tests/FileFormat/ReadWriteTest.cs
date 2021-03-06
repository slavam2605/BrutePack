﻿using System;
using System.IO;
using System.Linq;
using BrutePack.Decompression;
using BrutePack.FileFormat;
using NUnit.Framework;

namespace BrutePack_Tests.FileFormat
{
    [TestFixture]
    public class ReadWriteTest
    {
        private const int TestBlockSize = 1000;

        [Test]
        [ExpectedException(typeof(ApplicationException))]
        public void TestUnknownBlock()
        {
            var memStream = new MemoryStream();
            memStream.WriteByte(127);
            memStream.WriteByte(0);
            memStream.WriteByte(0);
            memStream.WriteByte(0);
            memStream.Position = 0;
            var block = new BinaryReader(memStream).ReadBlock();
            BlockDecompressor.Decompress(block);
        }

        [Test]
        public void TestReadWriteRoundtrip()
        {
            var block = MakeTestBlock();

            var memStream = new MemoryStream(TestBlockSize + 3);
            var writer = new BinaryWriter(memStream);
            writer.WriteBrutePackBlock(block);
            var reader = new BinaryReader(memStream);
            memStream.Seek(0, SeekOrigin.Begin);

            var readBlock = reader.ReadBlock();

            Assert.AreEqual(readBlock.BlockType, block.BlockType);
            Assert.AreEqual(readBlock.BlockData, block.BlockData);
        }

        [Test]
        public void TestUncompressStream()
        {
            var block = MakeTestBlock();

            var memStream = new MemoryStream(TestBlockSize + 3);
            var writer = new BinaryWriter(memStream);
            writer.WriteBrutePackBlock(block);
            memStream.Seek(0, SeekOrigin.Begin);

            var uncompressStream = new BruteUncompressingStream(new BinaryReader(memStream));
            var uncompressReader = new BinaryReader(uncompressStream);

            var readBytes = uncompressReader.ReadBytes(TestBlockSize);

            Assert.AreEqual(readBytes, block.BlockData);
        }

        [Test]
        public void TestUncompressStreamManyBlocks()
        {
            var block = MakeTestBlock();

            var memStream = new MemoryStream(TestBlockSize + 3);
            var writer = new BinaryWriter(memStream);
            writer.WriteBrutePackBlock(block);
            writer.WriteBrutePackBlock(block);
            memStream.Seek(0, SeekOrigin.Begin);

            var uncompressStream = new BruteUncompressingStream(new BinaryReader(memStream));
            var uncompressReader = new BinaryReader(uncompressStream);

            var readBytes = uncompressReader.ReadBytes(TestBlockSize * 2);

            Assert.AreEqual(readBytes.Take(TestBlockSize).ToArray(), block.BlockData);
            Assert.AreEqual(readBytes.Skip(TestBlockSize).Take(TestBlockSize).ToArray(), block.BlockData);
        }
        [Test]
        public void TestUncompressStreamManyLargeBlocks()
        {
            var block = MakeTestBlock(100);

            var memStream = new MemoryStream();
            var writer = new BinaryWriter(memStream);
            writer.WriteBrutePackBlock(block);
            writer.WriteBrutePackBlock(block);
            memStream.Seek(0, SeekOrigin.Begin);

            var uncompressStream = new BruteUncompressingStream(new BinaryReader(memStream));
            var uncompressReader = new BinaryReader(uncompressStream);

            var readBytes = uncompressReader.ReadBytes(TestBlockSize * 200);

            Assert.AreEqual(readBytes.Take(TestBlockSize * 100).ToArray(), block.BlockData);
            Assert.AreEqual(readBytes.Skip(TestBlockSize * 100).Take(TestBlockSize * 100).ToArray(), block.BlockData);
        }

        private static BrutePackBlock MakeTestBlock(int sizeMultiplier = 1)
        {
            var block = new BrutePackBlock(BlockType.Uncompressed, new byte[TestBlockSize * sizeMultiplier]);
            for (int i = 0; i < TestBlockSize * sizeMultiplier; i++)
            {
                block.BlockData[i] = (byte) (i * 5);
            }
            return block;
        }
    }
}