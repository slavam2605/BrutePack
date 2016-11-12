using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace BrutePack.DataStructure
{
    public class BlockList<T> : IList<T>
    {
        private const int BlockPower = 17;
        private const int BlockSize = 1 << BlockPower;
        private readonly List<T[]> data;
        private int currentBlock;
        private int posInBlock;
        private int length;

        public BlockList()
        {
            data = new List<T[]> {new T[BlockSize]};
            currentBlock = 0;
            posInBlock = 0;
            length = 0;
        }

        private void AllocateNewBlock()
        {
            data.Add(new T[BlockSize]);
            currentBlock++;
            posInBlock = 0;
        }

        public void Add(T item)
        {
            if (posInBlock >= BlockSize)
                AllocateNewBlock();
            data[currentBlock][posInBlock++] = item;
            length++;
        }

        public void AddArray(T[] array)
        {
            var rest = array.Length;
            while (rest > BlockSize - posInBlock)
            {
                Array.Copy(array, array.Length - rest, data[currentBlock], posInBlock, BlockSize - posInBlock);
                rest -= BlockSize - posInBlock;
                length += BlockSize - posInBlock;
                AllocateNewBlock();
            }
            Array.Copy(array, array.Length - rest, data[currentBlock], posInBlock, rest);
            posInBlock += rest;
            length += rest;
        }

        public T this[int index]
        {
            get
            {
                var y = index >> BlockPower;
                return data[y][index - y * BlockSize];
            }
            set
            {
                var y = index >> BlockPower;
                data[y][index - y * BlockSize] = value;
            }
        }

        public T[] ToArray()
        {
            var result = new T[length];
            for (var y = 0; y < data.Count - 1; y++)
            {
                data[y].CopyTo(result, BlockSize * y);
            }
            var flushed = BlockSize * (data.Count - 1);
            for (var x = 0; x < length - flushed; x++)
            {
                result[flushed + x] = data[data.Count - 1][x];
            }
            return result;
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public void Clear()
        {
            throw new NotImplementedException();
        }
        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }
        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }
        public int Count => length;
        public bool IsReadOnly { get; }
        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }
        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }
        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }
    }
}