using SemestralnaPracaAUS2.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace SemestralnaPracaAUS2.Structures
{
    public class AVLTree<T>: BST<T>, IMyStructure<T> where T : IMyComparable<T>
    {
        protected override AvlNode NewNode(T value, Node? parent) => new AvlNode(value, parent);
        protected static AvlNode? A(Node? n) => (AvlNode?)n;
        public override bool Add(T value) 
        {
            if (root == null)
            {
                root = NewNode(value, null);
                count++;
                return true;
            }

            //var path = new Stack<StackNode>();
            Node? curr = root;
            Node? parent = null;
            bool lastStepLeft = false;

            while (curr != null)
            {
                parent = curr;
                int cmp = Compare(value, curr.Value);
                if (cmp == 0) return false; 

                if (cmp < 0)
                {
                    // pôjdeme doľava
                    //path.Push(new StackNode(A(parent)!, left: true));
                    lastStepLeft = true;
                    curr = curr.Left;
                }
                else
                {
                    // pôjdeme doprava
                    //path.Push(new StackNode(A(parent)!, left: false));
                    lastStepLeft = false;
                    curr = curr.Right;
                }
            }

            // 2) Vloženie nového uzla
            var newNode = (AvlNode)NewNode(value, parent);
            if (lastStepLeft) parent!.Left = newNode;
            else parent!.Right = newNode;
            count++;

            // 3) Rebalans bez zásobníka – kráčame hore cez Parent
            var child = A(newNode)!;
            var node = A(newNode.Parent);

            while (node != null)
            {
                // aktualizácia BF podľa toho, kam sme práve vložili/skrátka rástli
                if (child == node.Left) node.Balance -= 1;  // ľavá vetva narástla
                else node.Balance += 1;                     // pravá vetva narástla

                // a) po aktualizácii BF je 0 → výška podstromu sa nezmenila, končíme
                if (node.Balance == 0)
                    break;

                // b) BF je ±1 → výška tohto uzla narástla o 1, pokračujeme k predkovi
                if (node.Balance == -1 || node.Balance == +1)
                {
                    child = node;
                    node = A(node.Parent);
                    continue;
                }

                // c) Prekročenie: BF == -2 alebo +2 → vykonaj rotáciu a končíme
                if (node.Balance == -2)
                {
                    var left = A(node.Left);
                    if (left != null && left.Balance <= 0)
                    {
                        // LL
                        RotateRight(node);
                    }
                    else
                    {
                        // LR
                        RotateLeftRight(node);
                    }
                    break; // po rotácii pri vkladaní končíme
                }
                else // node.Balance == +2
                {
                    var right = A(node.Right);
                    if (right != null && right.Balance >= 0)
                    {
                        // RR
                        RotateLeft(node);
                    }
                    else
                    {
                        // RL
                        RotateRightLeft(node);
                    }
                    break; // po rotácii pri vkladaní končíme
                }
            }

            return true;
        }
        public override bool Delete(T value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            var path = new Stack<StackNode>();
            AvlNode? target = FindTargetAndFillPath(value, path);
            if (target == null)
                return false;

            AvlNode? replacement = FindReplacementAndFillPath(target, path);
            if (replacement != null)
            {
                // prepíš hodnotu cieľa a vytrhni náhradníka
                RewriteNode(target, replacement);
                RemoveNodeWithAtMostOneChild(replacement);
                count--;

                RebalanceUsingPath(path);
                return true;
            }
            else
            {
                // target je LIST → odpoj + rebalans
                RemoveNodeWithAtMostOneChild(target);
                count--;

                RebalanceUsingPath(path);
                return true;
            }
        }
        public override bool Find(T value, out T found) 
        {
            var ok = base.Find(value, out found);
            return ok;
        }
        public override bool FindMin(out T found)
        {
            return base.FindMin(out found);
        }
        public override bool FindMax(out T found)
        {
            return base.FindMax(out found);
        }
        private void RotateRightLeft(AvlNode node)
        {
            var y = A(node.Right);                 // pravý syn
            if (y == null) throw new InvalidOperationException("RL: right child is null.");
            var x = A(y.Left);                     // stredový uzol
            if (x == null) throw new InvalidOperationException("RL: right-left middle is null.");

            int xBal = x.Balance;                  // rozhoduje o BF po rotácii

            // podstromy, ktoré sa presúvajú
            var B = A(x.Left);                     // pôjde pod node.Right
            var C = A(x.Right);                    // pôjde pod y.Left

            // 1) x ide na miesto node
            x.Parent = node.Parent;
            if (node.Parent == null) root = x;
            else if (node.Parent.Left == node) node.Parent.Left = x;
            else node.Parent.Right = x;

            // 2) node sa stane ľavým synom x; jeho pravý podstrom bude B
            x.Left = node;
            node.Parent = x;
            node.Right = B;
            if (B != null) B.Parent = node;

            // 3) y sa stane pravým synom x; jeho ľavý podstrom bude C
            x.Right = y;
            y.Parent = x;
            y.Left = C;
            if (C != null) C.Parent = y;

            // --- Nastavenie BF podľa pôvodného x.Balance (pravidlá pre RL) ---
            // zrkadlo k LR (s tvojím znamienkom: pravá = +, ľavá = −)
            // xBal > 0  => node.Balance = -1, y.Balance = 0
            // xBal == 0 => node.Balance =  0, y.Balance = 0
            // xBal < 0  => node.Balance =  0, y.Balance = +1
            if (xBal > 0)
            {
                node.Balance = -1;
                y.Balance = 0;
            }
            else if (xBal == 0)
            {
                node.Balance = 0;
                y.Balance = 0;
            }
            else // xBal < 0
            {
                node.Balance = 0;
                y.Balance = +1;
            }
            x.Balance = 0;
        }
        private void RotateLeftRight(AvlNode node) 
        {
            var leftSon = A(node.Left);           // ľavý syn
            if (leftSon == null) throw new InvalidOperationException("LR: left child is null.");
            var middleNode = A(leftSon.Right);             // stredový uzol
            if (middleNode == null) throw new InvalidOperationException("LR: left-right middle is null.");

            int xBal = middleNode.Balance;

            var B = A(middleNode.Left);              
            var C = A(middleNode.Right);             

            // 1) x ide hore na miesto node
            middleNode.Parent = node.Parent;
            if (node.Parent == null) root = middleNode;
            else if (node.Parent.Left == node) node.Parent.Left = middleNode;
            else node.Parent.Right = middleNode;

            // 2) y sa stane ľavým synom x; jeho pravý podstrom bude B
            middleNode.Left = leftSon;
            leftSon.Parent = middleNode;
            leftSon.Right = B;
            if (B != null) B.Parent = leftSon;

            // 3) node sa stane pravým synom x; jeho ľavý podstrom bude C
            middleNode.Right = node;
            node.Parent = middleNode;
            node.Left = C;
            if (C != null) C.Parent = node;

            if (xBal < 0)
            {
                node.Balance = +1;
                leftSon.Balance = 0;
            }
            else if (xBal == 0)
            {
                node.Balance = 0;
                leftSon.Balance = 0;
            }
            else // xBal > 0
            {
                node.Balance = 0;
                leftSon.Balance = -1;
            }
            middleNode.Balance = 0;
        }
        public override List<T> LevelOrderList() 
        {
            return base.LevelOrderList();
        }
        private void DecideRotation(AvlNode node) 
        {
            if (node.Balance == -2)
            {
                var left = A(node.Left);
                if (left != null)
                {
                    if (left.Balance <= 0)
                    {
                        RotateRight(node);
                    }
                    else
                    {
                        RotateLeftRight(node);
                    }
                }
            }
            else if (node.Balance == 2) 
            {
                var right = A(node.Right);
                if (right != null)
                {
                    if (right.Balance >= 0)
                    {
                        RotateLeft(node);
                    }
                }
                else 
                {
                    RotateRightLeft(node);
                }
            }
        }
        private void RotateRight(AvlNode node)
        {
            var leftSubTree = A(node.Left); // y
            if (leftSubTree == null)
                throw new InvalidOperationException("RotateRight: left child is null.");

            var newLeftSubTree = A(leftSubTree.Right); // B

            // y ide na miesto node
            leftSubTree.Parent = node.Parent;
            if (node.Parent == null)
            {
                root = leftSubTree; // node bol koreň
            }
            else if (node.Parent.Left == node)
            {
                node.Parent.Left = leftSubTree;
            }
            else
            {
                node.Parent.Right = leftSubTree;
            }

            // node sa stane pravým synom y
            leftSubTree.Right = node;
            node.Parent = leftSubTree;

            // B (pôvodný pravý podstrom y) sa stane ľavým podstromom node
            node.Left = newLeftSubTree;
            if (newLeftSubTree != null) newLeftSubTree.Parent = node;

            // --- Aktualizácia BF podľa tvojej konvencie (+ vpravo, − vľavo) ---

            if (leftSubTree.Balance == 0)
            {
                // špeciál pri mazaní
                node.Balance = -1;
                leftSubTree.Balance = +1;
            }
            else if (leftSubTree.Balance < 0)
            {
                // jednoduchý LL prípad (vkladanie) → po rotácii oba BF = 0
                node.Balance = 0;
                leftSubTree.Balance = 0;
            }
            else
            {
                // leftSubTree.Balance > 0  => LR prípad (dvojitá rotácia)
                System.Diagnostics.Debug.Fail("RotateRight called for LR case – najprv RotateLeft(y), potom RotateRight(node).");
                node.Balance = 0;           // dočasné „safe defaults“
                leftSubTree.Balance = 0;
            }
        }
        public IEnumerable<T> Range(T low, T high) 
        {
            return base.Range(low, high);
        }
        private void RebalanceUsingPath(Stack<StackNode> path)
        {
            while (path.Count > 0)
            {
                var frame = path.Pop();
                var node = frame.Node;

                int oldBF = node.Balance;

                // skrátila sa ľavá vetva (frame.Left==true) → BF += +1
                // skrátila sa pravá vetva (frame.Left==false) → BF += -1
                node.Balance += frame.Left ? +1 : -1;

                if (node.Balance == 0)
                {
                    // podstrom sa skrátil o 1 → pokračuj k ďalšiemu predkovi
                    continue;
                }

                // 0 -> ±1: výška uzla sa nezmenila → končíme (PDF – stop condition)
                if (oldBF == 0 && (node.Balance == 1 || node.Balance == -1))
                    break;

                if (node.Balance == -2)
                {
                    var left = A(node.Left);
                    if (left != null && left.Balance <= 0)
                    {
                        // LL: jednoduchá PRAVÁ rotácia
                        // Delete-špeciál: ak left.Balance == 0 → po rotácii STOP, inak pokračuj
                        bool stop = (left.Balance == 0);
                        RotateRight(node);
                        if (stop) break; else continue;
                    }
                    else
                    {
                        // LR: DVOJITÁ rotácia (ĽAVO-PRAVÁ) → po rotácii pokračuj
                        RotateLeftRight(node);
                        continue;
                    }
                }
                else if (node.Balance == +2)
                {
                    var right = A(node.Right);
                    if (right != null && right.Balance >= 0)
                    {
                        // RR: jednoduchá ĽAVÁ rotácia
                        // Delete-špeciál: ak right.Balance == 0 → po rotácii STOP, inak pokračuj
                        bool stop = (right.Balance == 0);
                        RotateLeft(node);
                        if (stop) break; else continue;
                    }
                    else
                    {
                        // RL: DVOJITÁ rotácia (PRAVO-ĽAVÁ) → po rotácii pokračuj
                        RotateRightLeft(node);
                        continue;
                    }
                }

                // nový BF je ±1 (a oldBF nebol 0) → výška sa už nemení → končíme
                break;
            }
        }
        private AvlNode? FindReplacementAndFillPath(AvlNode target, Stack<StackNode> path)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (path == null) throw new ArgumentNullException(nameof(path));

            bool hasLeft = target.Left != null;
            bool hasRight = target.Right != null;

            if (!hasLeft && !hasRight)
                return null;

            if (hasLeft)
            {
                path.Push(new StackNode(A(target)!, left: true));
                Node? n = target.Left;

                while (n!.Right != null)
                {
                    path.Push(new StackNode(A(n)!, left: false));
                    n = n.Right;
                }
                return A(n);
            }

            path.Push(new StackNode(A(target)!, left: false));
            Node? m = target.Right;

            while (m!.Left != null)
            {
                path.Push(new StackNode(A(m)!, left: true));
                m = m.Left;
            }
            return A(m);
        }
        private AvlNode? FindTargetAndFillPath(T value, Stack<StackNode> path)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (path == null) throw new ArgumentNullException(nameof(path));

            path.Clear();                 // nech je cesta vždy čistá
            Node? curr = root;

            while (curr != null)
            {
                int cmp = Compare(value, curr.Value);
                if (cmp == 0)
                {
                    return A(curr);
                }

                if (cmp < 0)
                {
                    path.Push(new StackNode(A(curr)!, left: true));
                    curr = curr.Left;
                }
                else
                {
                    path.Push(new StackNode(A(curr)!, left: false));
                    curr = curr.Right;
                }
            }
            return null;
        }
        private void RotateLeft(AvlNode node) 
        {
            var rightSubTree = A(node.Right);//y
            var newRightSubTree = A(rightSubTree.Left);//b
            rightSubTree.Parent = node.Parent;
            if (node.Parent == null)
            {
                root = rightSubTree;//node bol koren takze teraz bude koren rightSubTree
            }
            else if (node.Parent.Left == node)
            {
                node.Parent.Left = rightSubTree;
            }
            else 
            { 
                node.Parent.Right = rightSubTree;
            }
            rightSubTree.Left = node;
            node.Parent = rightSubTree;
            node.Right = newRightSubTree;
            if (newRightSubTree != null) 
            {
                newRightSubTree.Parent = node;
            }

            if (rightSubTree.Balance == 0)
            {
                node.Balance = 1;
                rightSubTree.Balance = -1;
            }
            else if (rightSubTree.Balance > 0)
            {
                node.Balance = 0;
                rightSubTree.Balance = 0;
            }
            else 
            {
                System.Diagnostics.Debug.Fail("RotateLeft called for RL case – use double rotation (Right(y) then Left(n)).");
                node.Balance = 0;
                rightSubTree.Balance = 0;
            }
        }
        private struct StackNode
        {
            public AvlNode Node;   // uzol, z ktorého sme schádzali
            public bool Left;      // true = šli sme doľava; false = doprava
            public StackNode(AvlNode node, bool left) { Node = node; Left = left; }
        }
        protected class AvlNode : Node
        {
            public int Balance; // H(L) - H(P); v AVL má zostať v rozsahu { -1, 0, +1 }
            public AvlNode(T value, Node? parent = null) : base(value, parent)
            {
                Balance = 0;
            }
        }
    } 
}
