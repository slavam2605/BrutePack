using System;
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
            var path = TestUtil.GetTestDataDir() + "test.txt";
            Console.WriteLine(path);
            Console.WriteLine(File.Open(path, FileMode.Open, FileAccess.Read));
            Console.WriteLine("Test running");
            Assert.AreEqual(1, 1);
        }
    }
}