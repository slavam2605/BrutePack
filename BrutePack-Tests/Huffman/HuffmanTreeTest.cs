using System;
using System.Globalization;
using System.Linq;
using BrutePack.Huffman;
using NUnit.Framework;

namespace BrutePack_Tests.Huffman
{
    [TestFixture]
    public class HuffmanTreeTest
    {
        [Test]
        public void TestEncodeCustom()
        {
            var tree1 = new HuffmanTree();
            for (var i = 0; i < 15; i++)
            {
                tree1.AddCode(Replicate(i, '1') + "0", 'a' + i);
            }
            var lookupTable = tree1.GetLookupTable(255);
            const string test = "abacabaabcdefghijklmno";
            var result1 = "";
            var result2 = "";
            foreach (var c in test)
            {
                result1 += Adjust(Convert.ToString(lookupTable.Item1[c], 2), lookupTable.Item2[c], '0');
                result2 += Replicate(c - 'a', '1') + "0";
            }
            Assert.AreEqual(result1, result2);
        }

        private static string Adjust(string s, int n, char c)
        {
            while (s.Length < n)
            {
                s += c;
            }
            return s;
        }

        [Test]
        public void TestDecodeCustom()
        {
            var tree1 = new HuffmanTree();
            var tree2 = new HuffmanTreeSlow();
            var tree3 = new HuffmanTreeTree();
            for (var i = 0; i < 15; i++)
            {
                tree1.AddCode(Replicate(i, '1') + "0", 'a' + i);
                tree2.AddCode(Replicate(i, '1') + "0", 'a' + i);
                tree3.AddCode(Replicate(i, '1') + "0", 'a' + i);
            }
            var lookupTable = tree1.GetLookupTable(255);
            const string test = "abacabaabcdefghijklmno";
            var encoded = test.Aggregate("", (current, c) =>
                    current + Adjust(Convert.ToString(lookupTable.Item1[c], 2), lookupTable.Item2[c], '0')
            );
            var decoder1 = tree1.GetDecoder();
            var decoder2 = tree2.GetDecoder();
            var decoder3 = tree3.GetDecoder();
            var result1 = "";
            var result2 = "";
            var result3 = "";
            foreach (var c in encoded)
            {
                var r1 = decoder1.Next((byte) (c == '0' ? 0 : 1));
                var r2 = decoder2.Next((byte) (c == '0' ? 0 : 1));
                var r3 = decoder3.Next((byte) (c == '0' ? 0 : 1));
                if (r1 >= 0)
                    result1 += (char) r1;
                if (r2 >= 0)
                    result2 += (char) r2;
                if (r3 >= 0)
                    result3 += (char) r3;
            }
            Assert.AreEqual(test, result1);
            Assert.AreEqual(test, result2);
            Assert.AreEqual(test, result3);
        }

        private static string Replicate(int n, char c)
        {
            var s = "";
            for (var i = 0; i < n; i++)
                s += c;
            return s;
        }
    }
}