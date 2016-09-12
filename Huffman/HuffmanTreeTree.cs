using System;

namespace BrutePack.Huffman
{
    public class HuffmanTreeTree
    {
        private readonly Node root;
        public static readonly HuffmanTreeTree StaticTree;

        static HuffmanTreeTree()
        {
            StaticTree = new HuffmanTreeTree();
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

        public HuffmanTreeTree()
        {
            root = new Node();
        }

        public void AddCode(string code, int value)
        {
            var node = root;
            foreach (var c in code)
            {
                if (c == '0')
                {
                    if (node.left == null)
                    {
                        node.left = new Node();
                    }
                    node = node.left;
                }
                else
                {
                    if (node.right == null)
                    {
                        node.right = new Node();
                    }
                    node = node.right;
                }
            }
            node.value = value;
        }

        public HuffmanDecoder GetDecoder()
        {
            return new HuffmanDecoder(root);
        }

        public class HuffmanDecoder
        {
            private readonly Node root;
            private Node state;

            public HuffmanDecoder(HuffmanTreeTree.Node root)
            {
                this.root = root;
                state = root;
            }

            public int Next(int nextDigit)
            {
                state = nextDigit == 0 ? state.left : state.right;
                var value = state.value;
                if (value >= 0)
                    state = root;
                return value;
            }
        }

        public class Node
        {
            public Node left;
            public Node right;
            public int value = -1;
        }
    }
}