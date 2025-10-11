using SemestralnaPracaAUS2.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemestralnaPracaAUS2.Structures
{
    public class BST<T> where T : IMyComparable<T>
    {
        private BSTNode? root;
        private int count;

        public BST()
        {
            root = null;
            count = 0;
        }
        public bool Add(T value) 
        {
            if (value == null)
                throw new System.ArgumentNullException(nameof(value));

            if (root == null)
            {
                root = new BSTNode(value, parent: null);
                return true;
            }

            var current = root;
            BSTNode? parent = null;
            while (current != null) 
            {
                parent = current;
                int cmp = Compare(value, current.Value);
                //TODO: ked budu hotove duplicity dokoncit tuto moznost 
                if (cmp == 0)
                {
                    return false;
                }
                else if (cmp > 0)
                {
                    current = current.Right;
                }
                else 
                {
                    current = current.Left;
                }
            }

            var newNode = new BSTNode(value, parent);
            if (Compare(value, parent!.Value) < 0)
                parent.Left = newNode;
            else
                parent.Right = newNode;
            count++;
            return true;

        }

        public bool Find(T value) 
        {
            return false;
        }

        private static int Compare(T a, T b) => a.CompareTo(b);

        private sealed class BSTNode 
        {
            public T Value;
            public BSTNode? Left { get; set; }
            public BSTNode? Right { get; set; }
            public BSTNode? Parent { get; set; }

            public BSTNode(T value, BSTNode? parent = null)
            {
                Value = value;
                Parent = parent;
            }
        }
    }
}
