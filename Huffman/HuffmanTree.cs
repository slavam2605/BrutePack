using System;
using System.Linq;

namespace BrutePack.Huffman
{
    public class HuffmanTree
    {
        private readonly int[] tree;
        public static readonly HuffmanTree StaticTree;

        static HuffmanTree()
        {
            StaticTree = new HuffmanTree(new int[1024], 1024);
            for (var code = 48; code < 192; code++)
            {
                var stringCode = Convert.ToString(code, 2);
                while (stringCode.Length < 8)
                    stringCode = "0" + stringCode;
                StaticTree.AddCode(stringCode, code - 48);
            }
            for (var code = 400; code < 512; code++)
            {
                var stringCode = Convert.ToString(code, 2);
                StaticTree.AddCode(stringCode, code - 256);
            }
            for (var code = 0; code < 24; code++)
            {
                var stringCode = Convert.ToString(code, 2);
                while (stringCode.Length < 7)
                    stringCode = "0" + stringCode;
                StaticTree.AddCode(stringCode, code + 256);
            }
            for (var code = 192; code < 200; code++)
            {
                var stringCode = Convert.ToString(code, 2);
                StaticTree.AddCode(stringCode, code + 88);
            }
        }

        public HuffmanTree()
        {
            tree = new int[65536];
            for (var i = 0; i < tree.Length; i++)
                tree[i] = -1;
        }

        public HuffmanTree(int[] tree, int length)
        {
            this.tree = tree;
            for (var i = 0; i < length; i++)
                tree[i] = -1;
        }

        public void AddCode(string code, int value)
        {
            var index = code.Aggregate(0, (current, b) => 2 * current + 1 + (b - '0'));
            tree[index] = value;
        }

        public HuffmanDecoder GetDecoder()
        {
            return new HuffmanDecoder(tree);
        }

        public class HuffmanDecoder
        {
            private readonly int[] tree;
            private int state;

            public HuffmanDecoder(int[] tree)
            {
                this.tree = tree;
                state = 0;
            }

            public int Next(byte nextDigit)
            {
                state = 2 * state + 1 + nextDigit;
                var value = tree[state];
                if (value >= 0)
                    state = 0;
                return value;
            }
        }
    }
}