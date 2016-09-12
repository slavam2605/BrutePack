using System;
using System.Collections.Generic;

namespace BrutePack.Huffman
{
    public class HuffmanTreeSlow
    {
        private readonly Dictionary<string, int> codes;
        public static readonly HuffmanTreeSlow StaticTree;

        static HuffmanTreeSlow()
        {
            StaticTree = new HuffmanTreeSlow();
            for (int code = 48; code < 192; code++)
            {
                string stringCode = Convert.ToString(code, 2);
                while (stringCode.Length < 8)
                    stringCode = "0" + stringCode;
                StaticTree.AddCode(stringCode, code - 48);
            }
            for (int code = 400; code < 512; code++)
            {
                string stringCode = Convert.ToString(code, 2);
                StaticTree.AddCode(stringCode, code - 256);
            }
            for (int code = 0; code < 24; code++)
            {
                string stringCode = Convert.ToString(code, 2);
                while (stringCode.Length < 7)
                    stringCode = "0" + stringCode;
                StaticTree.AddCode(stringCode, code + 256);
            }
            for (int code = 192; code < 200; code++)
            {
                string stringCode = Convert.ToString(code, 2);
                StaticTree.AddCode(stringCode, code + 88);
            }
        }

        public HuffmanTreeSlow()
        {
            codes = new Dictionary<string, int>();
        }

        public void AddCode(string code, int value)
        {
            codes[code] = value;
        }

        public HuffmanDecoder GetDecoder()
        {
            return new HuffmanDecoder(this);
        }

        public override string ToString()
        {
            return string.Join(";", codes);
        }

        public class HuffmanDecoder
        {
            private readonly HuffmanTreeSlow tree;
            private string state = "";

            public HuffmanDecoder(HuffmanTreeSlow tree)
            {
                this.tree = tree;
            }

            public int Next(int nextDigit)
            {
                state += nextDigit;
                int value;
                try
                {
                    value = tree.codes[state];
                    state = "";
                }
                catch (KeyNotFoundException e)
                {
                    value = -1;
                }
                return value;
            }
        }
    }
}