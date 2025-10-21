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
        protected Node? root;
        public int count;
        public BST()
        {
            root = null;
            count = 0;
        }
        protected virtual Node NewNode(T value, Node? parent) => new Node(value, parent);
        public virtual bool Add(T value) 
        {
            if (value == null)
                throw new System.ArgumentNullException(nameof(value));

            if (root == null)
            {
                root = new Node(value, parent: null);
                count++;
                return true;
            }

            var current = root;
            Node? parent = null;
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

            var newNode = new Node(value, parent);
            if (Compare(value, parent!.Value) < 0)
                parent.Left = newNode;
            else
                parent.Right = newNode;

            count++;
            return true;

        }
        public virtual bool Find(T value, out T found) 
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
        public virtual bool Delete(T value) 
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
        public virtual bool FindMin(out T found)
        {
            if (root == null)
            {
                found = default!;
                return false;
            }

            var curr = root;
            while (curr.Left != null)
                curr = curr.Left;

            found = curr.Value;
            return true;
        }
        public virtual bool FindMax(out T found)
        {
            if (root == null)
            {
                found = default!;
                return false;
            }

            var curr = root;
            while (curr.Right != null)
                curr = curr.Right;

            found = curr.Value;
            return true;
        }
        public virtual IEnumerable<T> Range(T low, T high)
        {
            if (Compare(low, high) > 0) yield break; // prázdny interval

            var stack = new Stack<Node>();
            var curr = root;

            while (stack.Count > 0 || curr != null)
            {
                // zostup doľava len ak môže byť ešte >= low
                while (curr != null)
                {
                    // ak je curr.Value < low, všetko vľavo je ešte menšie → preskoč vľavo
                    if (Compare(curr.Value, low) < 0)
                    {
                        curr = curr.Right;
                    }
                    else
                    {
                        stack.Push(curr);
                        curr = curr.Left;
                    }
                }
                if (stack.Count == 0) yield break;
                curr = stack.Pop();

                // inorder ide vzostupne; keď presiahneme high, sme hotoví
                if (Compare(curr.Value, high) > 0)
                    yield break;

                // sme v intervale
                yield return curr.Value;

                // doprava má zmysel len ak curr.Value <= high
                curr = curr.Right;
            }
        }
        protected virtual void RewriteNode(Node oldNode, Node newNode) 
        {
            oldNode.Value = newNode.Value;
        }
        private Node FindMinInRightSubTree(Node node) 
        {
            var target = node.Right;
            while (target.Left != null) 
            {
                target = target.Left;
            }
            return target;
        }
        private Node FindMaxInLeftSubTree(Node node)
        {
            var target = node.Left;
            while (target.Right != null)
            {
                target = target.Right;
            }
            return target;
        }
        private Node FindNode(T value) 
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
        private void DetachLeaf(Node node)
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
        public virtual List<T> LevelOrderList()
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

            var q = new Queue<Node>();
            q.Enqueue(root);

            while (q.Count > 0)
            {
                var node = q.Dequeue();
                yield return node.Value;

                if (node.Left != null) q.Enqueue(node.Left);
                if (node.Right != null) q.Enqueue(node.Right);
            }
        }
        protected virtual void RemoveNodeWithAtMostOneChild(Node target)
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
        private void ClearChildrenTarget(Node target) 
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
        protected static int Compare(T a, T b) => a.CompareTo(b);
        protected class Node 
        {
            public T Value;
            public Node? Left { get; set; }
            public Node? Right { get; set; }
            public Node? Parent { get; set; }

            public Node(T value, Node? parent = null)
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
