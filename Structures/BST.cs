using SemestralnaPracaAUS2.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SemestralnaPracaAUS2.Structures
{
    public class BST<T> where T : IMyComparable<T>
    {
        private BSTNode? root;
        public int count;

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
                count++;
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

        public bool Find(T value, out T found) 
        {
            var current  = root;
            while (current != null) 
            { 
                int cmp = Compare(value, current.Value);

                if (cmp == 0)
                {
                    found = current.Value;
                    return true;
                }
                else if (cmp < 0)
                {
                    current = current.Left;
                }
                else 
                {
                    current = current.Right;
                }
            }
            found = default;
            return false;
        }

        public bool Delete(T value) 
        {
            var node = FindNode(value);
            if (node == null) return false;

            bool hasLeft = node.Left != null;
            bool hasRight = node.Right != null;

            if (!hasLeft && !hasRight)
            {
                DetachLeaf(node);
                count--;
                return true;
            }
            if (hasRight && !hasLeft)
            {
                //najde v pravo najmensieho
                var min = FindMinInRightSubTree(node);
                RewriteNode(node, min);
                RemoveNodeWithAtMostOneChild(min);
                count--;
                return true;
            }
            if (!hasRight && hasLeft)
            {
                //najde v lavo najvacsieho 
                var max = FindMaxInLeftSubTree(node);
                RewriteNode(node,max);
                RemoveNodeWithAtMostOneChild(max);
                count--;
                return true;
            }
            
            {
                var min = FindMinInRightSubTree(node);
                RewriteNode(node, min);
                RemoveNodeWithAtMostOneChild(min); 
                count--;
                return true;
            }
        }

        private void RewriteNode(BSTNode oldNode, BSTNode newNode) 
        {
            oldNode.Value = newNode.Value;
        }
        private BSTNode FindMinInRightSubTree(BSTNode node) 
        {
            var target = node.Right;
            while (target.Left != null) 
            {
                target = target.Left;
            }
            return target;
        }
        private BSTNode FindMaxInLeftSubTree(BSTNode node)
        {
            var target = node.Left;
            while (target.Right != null)
            {
                target = target.Right;
            }
            return target;
        }
        private BSTNode FindNode(T value) 
        {
            var current = root;
            while (current != null) 
            {
                int cmp = Compare(value, current.Value);
                if (cmp == 0)
                {
                    return current;
                }
                else if (cmp < 0)
                {
                    current = current.Left;
                }
                else
                {
                    current = current.Right;
                }
            }
            return null;
        }
        private void DetachLeaf(BSTNode node)
        {
            if (node.Left != null || node.Right != null)
                throw new InvalidOperationException("DetachLeaf: node nie je list.");

            var parent = node.Parent;
            if (parent == null)
            {
                // mazal si posledný uzol (root)
                root = null;
            }
            else
            {
                if (parent.Left == node) parent.Left = null;
                else if (parent.Right == node) parent.Right = null;
            }
            node.Parent = null;
        }
        public List<T> LevelOrderList()
        {
            var result = new List<T>();
            foreach (var v in LevelOrder())
                result.Add(v);
            return result;
        }
        public IEnumerable<T> LevelOrder()
        {
            if (root == null)
                yield break;

            var q = new Queue<BSTNode>();
            q.Enqueue(root);

            while (q.Count > 0)
            {
                var node = q.Dequeue();
                yield return node.Value;

                if (node.Left != null) q.Enqueue(node.Left);
                if (node.Right != null) q.Enqueue(node.Right);
            }
        }

        private void RemoveNodeWithAtMostOneChild(BSTNode target)
        {
            var child = target.Left ?? target.Right; // buď jedno dieťa, alebo null (list)

            if (target.Parent == null)
            {
                // target je root
                root = child;
                if (child != null) child.Parent = null;
            }
            else if (target.Parent.Left == target)
            {
                target.Parent.Left = child;
                if (child != null) child.Parent = target.Parent;
            }
            else
            {
                target.Parent.Right = child;
                if (child != null) child.Parent = target.Parent;
            }

            // odpojený uzol už nikam neukazuje
            target.Parent = null;
            target.Left = null;
            target.Right = null;
        }
        private void ClearChildrenTarget(BSTNode target) 
        {
            var parent = target.Parent;
            if (target.Left != null && target.Right != null)
            {
                //vynimka nemoze to nikdy nastat lebo musim mat bud minimalni alebo max prvok 
            }
            else if (target.Right != null)
            {
                var subTree = target.Right;
                parent.Left = subTree;
            }
            else if(target.Left != null)
            {
                var subTree = target.Left;
                parent.Right = subTree;
            }
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

            public bool IsLeaf() 
            {
                if (Left == null && Right == null)
                    return true;
                else
                    return false;
            }
        }
    }
}
