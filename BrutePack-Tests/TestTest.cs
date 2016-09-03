using System;
using NUnit.Framework;

namespace BrutePack_Tests
{
    [TestFixture]
    public class TestTest
    {
        [Test]
        public void RunSimpleTest()
        {
            Console.WriteLine("Test running");
            Assert.AreEqual(1, 1);
        }
    }
}