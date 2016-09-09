﻿using System;
using System.IO;
using NUnit.Framework;

namespace BrutePack_Tests
{
    [TestFixture]
    public class TestTest
    {
        [Test]
        public void RunSimpleTest()
        {
            Console.WriteLine(File.Open(TestUtil.GetTestDataDir() + "test.txt", FileMode.Open, FileAccess.Read));
            Console.WriteLine("Test running");
            Assert.AreEqual(1, 1);
        }
    }
}