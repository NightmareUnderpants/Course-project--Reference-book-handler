using CoreLogic.Structures;
using CoreLogic.Vector;
using System;
using System.Text;

namespace CoreLogic.Classes
{
    public class Tree<TKey> where TKey : IComparable<TKey>
    {
        public class TreeNode
        {
            public TKey Key { get; private set; }
            public Vector<Sales> Values { get; private set; }
            public TreeNode Parent { get; internal set; }
            public TreeNode Left { get; internal set; }
            public TreeNode Right { get; internal set; }
            private int Height { get; set; }
            public int Count { get; private set; }
            public Tree<TKey> Tree { get; private set; }

            public TreeNode(TKey key, Tree<TKey> tree)
            {
                Console.WriteLine($"TreeNode constructor: Creating node with key {key}");
                Key = key;
                Tree = tree;
                Height = 1;
                Values = new Vector<Sales>();
                Count = 0;
                Console.WriteLine($"TreeNode constructor: Node created with key {key}, Height={Height}, Count={Count}");
            }

            #region Main Func

            public void Add(TKey key, Sales value)
            {
                Console.WriteLine($"{key} TreeNode.Add: Starting add, Value: {value}");
                int cmp = key.CompareTo(Key);
                Console.WriteLine($"{key} Compare key:\tCompared key: {Key}");
                if (cmp == 0)
                {
                    Console.WriteLine($"{key} Result: keys matches. Add into vector.");
                    Values.Add(value);
                    UpdateMetrics();
                    Console.WriteLine($"{key} TreeNode.Add: Value added to existing node. New Values count: {Values.Count}");
                    return;
                }
                if (cmp < 0)
                {
                    Console.WriteLine($"{key} Result: new key is precedes. Go to LeftNode");
                    if (Left == null)
                    {
                        Console.WriteLine($"{key} TreeNode.Add: Creating new left node");
                        Left = CreateNode(key, value);
                    }
                    else
                    {
                        Console.WriteLine($"{key} TreeNode.Add: Recursing into left subtree");
                        Left.Add(key, value);
                    }
                }
                else
                {
                    Console.WriteLine($"{key} Result: new key is follow. Go to RightNode");
                    if (Right == null)
                    {
                        Console.WriteLine($"{key} TreeNode.Add: Creating new right node");
                        Right = CreateNode(key, value);
                    }
                    else
                    {
                        Console.WriteLine($"{key} TreeNode.Add: Recursing into right subtree");
                        Right.Add(key, value);
                    }
                }
                Console.WriteLine($"{key} has been added");
                Rebalance();
                Console.WriteLine($"{key} TreeNode.Add: Completed add");
            }

            private TreeNode CreateNode(TKey key, Sales value)
            {
                Console.WriteLine($"{key} TreeNode.CreateNode: Creating new node");
                var node = new TreeNode(key, Tree) { Parent = this };
                node.Values.Add(value);
                node.UpdateMetrics();
                Console.WriteLine($"{key} TreeNode.CreateNode: Node created, Values count: {node.Values.Count}");
                return node;
            }

            public void Clear()
            {
                Console.WriteLine($"TreeNode.Clear: Starting clear for node {Key}");
                Left?.Clear();
                Right?.Clear();
                Left = Right = null;
                Values = new Vector<Sales>();
                Count = 0;
                Height = 1;
                Console.WriteLine($"TreeNode.Clear: Node {Key} cleared");
            }

            public bool Contains(TKey key)
            {
                Console.WriteLine($"TreeNode.Contains: Searching for key {key} in node {Key}");
                int cmp = key.CompareTo(Key);
                if (cmp == 0)
                {
                    Console.WriteLine($"TreeNode.Contains: Key {key} found in node {Key}");
                    return true;
                }
                bool result = cmp < 0 ? Left?.Contains(key) ?? false : Right?.Contains(key) ?? false;
                Console.WriteLine($"TreeNode.Contains: Key {key} {(result ? "found" : "not found")} in subtree of node {Key}");
                return result;
            }

            public bool Remove(TKey key)
            {
                Console.WriteLine($"{key} TreeNode.Remove: Starting remove from node {Key}");

                int cmp = key.CompareTo(Key);
                if (cmp < 0)
                {
                    Console.WriteLine($"{key} TreeNode.Remove: is less than current key {Key}, going left");
                    return Left?.Remove(key) ?? false;
                }
                if (cmp > 0)
                {
                    Console.WriteLine($"{key} TreeNode.Remove: is greater than current key {Key}, going right");
                    return Right?.Remove(key) ?? false;
                }

                Console.WriteLine($"{key} TreeNode.Remove: Found node to remove - Key: {Key}");
                Console.WriteLine($"{key} TreeNode.Remove: Left={(Left == null ? "null" : "exists")}, Right={(Right == null ? "null" : "exists")}");

                if (Left == null || Right == null)
                {
                    var child = Left ?? Right;
                    Console.WriteLine($"{key} TreeNode.Remove: Node has 0 or 1 children. Child={(child == null ? "null" : child.Key.ToString())}");

                    if (Parent == null)
                    {
                        Console.WriteLine($"{key} TreeNode.Remove: This is root node, setting tree root to child");
                        Tree.Root = child;
                        if (child != null) child.Parent = null;
                    }
                    else if (Parent.Left == this)
                    {
                        Console.WriteLine($"{key} TreeNode.Remove: This is left child of parent {Parent.Key}, replacing with child");
                        Parent.Left = child;
                    }
                    else
                    {
                        Console.WriteLine($"{key} TreeNode.Remove: This is right child of parent {Parent.Key}, replacing with child");
                        Parent.Right = child;
                    }
                    if (child != null) child.Parent = Parent;
                }
                else
                {
                    Console.WriteLine($"{key} TreeNode.Remove: Node has 2 children, finding predecessor");
                    var pred = Left;
                    while (pred.Right != null) pred = pred.Right;
                    Console.WriteLine($"{key} TreeNode.Remove: Predecessor found: {pred.Key}");

                    Key = pred.Key;
                    Values = pred.Values;
                    Console.WriteLine($"{key} TreeNode.Remove: Replaced with predecessor, now removing predecessor from original position");
                    pred.Remove(pred.Key);
                }

                Parent?.Rebalance();
                Console.WriteLine($"{key} TreeNode.Remove: Completed remove");
                return true;
            }

            #endregion

            #region Balancing and Control Func

            public void CopyTo(Sales[] array, int index)
            {
                Console.WriteLine($"TreeNode.CopyTo: Starting copy from node {Key}, starting index: {index}");
                Left?.CopyTo(array, index);
                if (Left != null) index += Left.Count;
                Console.WriteLine($"TreeNode.CopyTo: After left subtree, index: {index}");

                for (int i = 0; i < Values.Count; i++)
                {
                    array[index++] = Values[i];
                    Console.WriteLine($"TreeNode.CopyTo: Copied value {Values[i]} to index {index - 1}");
                }

                Right?.CopyTo(array, index);
                Console.WriteLine($"TreeNode.CopyTo: Completed copy from node {Key}");
            }

            public int IndexOf(TKey key)
            {
                Console.WriteLine($"TreeNode.IndexOf: Finding index of key {key} in node {Key}");
                int cmp = key.CompareTo(Key);
                if (cmp == 0)
                {
                    int leftCount = Left?.Count ?? 0;
                    Console.WriteLine($"TreeNode.IndexOf: Key {key} found at index {leftCount}");
                    return leftCount;
                }
                if (cmp < 0)
                {
                    Console.WriteLine($"TreeNode.IndexOf: Key {key} is less than current key, searching left subtree");
                    return Left?.IndexOf(key) ?? -1;
                }
                var baseIdx = (Left?.Count ?? 0) + Values.Count;
                Console.WriteLine($"TreeNode.IndexOf: Key {key} is greater than current key, base index: {baseIdx}");
                int idx = Right?.IndexOf(key) ?? -1;
                Console.WriteLine($"TreeNode.IndexOf: Right subtree returned index: {idx}");
                return idx < 0 ? -1 : baseIdx + idx;
            }

            private void Rebalance()
            {
                Console.WriteLine($"{Key} TreeNode.Rebalance: Starting rebalance");
                UpdateMetrics();
                int balance = (Left?.Height ?? 0) - (Right?.Height ?? 0);
                Console.WriteLine($"{Key} TreeNode.Rebalance: Balance factor: {balance}, Height: {Height}");

                if (balance > 1)
                {
                    Console.WriteLine($"{Key} TreeNode.Rebalance: Left heavy, checking left subtree balance");
                    if ((Left.Left?.Height ?? 0) < (Left.Right?.Height ?? 0))
                    {
                        Console.WriteLine($"{Key} TreeNode.Rebalance: Left-right case, performing left rotation on left child");
                        Left.RotateLeft();
                    }
                    Console.WriteLine($"{Key} TreeNode.Rebalance: Performing right rotation");
                    RotateRight();
                }
                else if (balance < -1)
                {
                    Console.WriteLine($"{Key} TreeNode.Rebalance: Right heavy, checking right subtree balance");
                    if ((Right.Right?.Height ?? 0) < (Right.Left?.Height ?? 0))
                    {
                        Console.WriteLine($"{Key} TreeNode.Rebalance: Right-left case, performing right rotation on right child");
                        Right.RotateRight();
                    }
                    Console.WriteLine($"{Key} TreeNode.Rebalance: Performing left rotation");
                    RotateLeft();
                }
                Console.WriteLine($"{Key} TreeNode.Rebalance: Completed rebalance");
            }

            private void UpdateMetrics()
            {
                Console.WriteLine($"{Key} TreeNode.UpdateMetrics: Updating metrics");
                int lh = Left?.Height ?? 0;
                int rh = Right?.Height ?? 0;
                Height = Math.Max(lh, rh) + 1;
                Count = (Left?.Count ?? 0) + (Right?.Count ?? 0) + Values.Count;
                Console.WriteLine($"{Key} TreeNode.UpdateMetrics: Height: {Height}, Count: {Count}, LeftHeight: {lh}, RightHeight: {rh}");
            }

            private void RotateLeft()
            {
                Console.WriteLine($"{Key} TreeNode.RotateLeft: Starting left rotation");
                var pivot = Right;
                Console.WriteLine($"{Key} TreeNode.RotateLeft: Pivot: {pivot.Key}");

                ReplaceInParent(pivot);
                Right = pivot.Left;
                if (pivot.Left != null) pivot.Left.Parent = this;
                pivot.Left = this;
                pivot.Parent = this.Parent;
                this.Parent = pivot;

                UpdateMetrics();
                pivot.UpdateMetrics();
                Console.WriteLine($"{Key} TreeNode.RotateLeft: Completed left rotation. New parent: {pivot.Key}");
            }

            private void RotateRight()
            {
                Console.WriteLine($"{Key} TreeNode.RotateRight: Starting right rotation");
                var pivot = Left;
                Console.WriteLine($"{Key} TreeNode.RotateRight: Pivot: {pivot.Key}");

                ReplaceInParent(pivot);
                Left = pivot.Right;
                if (pivot.Right != null) pivot.Right.Parent = this;
                pivot.Right = this;
                pivot.Parent = this.Parent;
                this.Parent = pivot;

                UpdateMetrics();
                pivot.UpdateMetrics();
                Console.WriteLine($"{Key} TreeNode.RotateRight: Completed right rotation. New parent: {pivot.Key}");
            }

            private void ReplaceInParent(TreeNode node)
            {
                Console.WriteLine($"TreeNode.ReplaceInParent: Replacing node {Key} with {node.Key} in parent");
                node.Parent = Parent;
                if (Parent == null)
                {
                    Console.WriteLine($"TreeNode.ReplaceInParent: This was root, setting tree root to {node.Key}");
                    Tree.Root = node;
                }
                else if (Parent.Left == this)
                {
                    Console.WriteLine($"TreeNode.ReplaceInParent: This was left child, parent left now points to {node.Key}");
                    Parent.Left = node;
                }
                else
                {
                    Console.WriteLine($"TreeNode.ReplaceInParent: This was right child, parent right now points to {node.Key}");
                    Parent.Right = node;
                }
            }

            #endregion

            #region Additional Remove Func

            internal void RemoveValues(Predicate<Sales> predicate)
            {
                Console.WriteLine($"TreeNode.RemoveValues: Starting to remove values from node {Key} with predicate");
                int initialCount = Values.Count;
                Console.WriteLine($"TreeNode.RemoveValues: Initial values count: {initialCount}");

                var newValues = new Vector<Sales>();
                for (int i = 0; i < Values.Count; i++)
                {
                    if (!predicate(Values[i]))
                    {
                        newValues.Add(Values[i]);
                        Console.WriteLine($"TreeNode.RemoveValues: Kept value: {Values[i]}");
                    }
                    else
                    {
                        Console.WriteLine($"TreeNode.RemoveValues: Removed value: {Values[i]}");
                    }
                }
                Values = newValues;
                Console.WriteLine($"TreeNode.RemoveValues: Final values count: {Values.Count}, removed: {initialCount - Values.Count}");

                var current = this;
                while (current != null)
                {
                    Console.WriteLine($"TreeNode.RemoveValues: Updating metrics for node {current.Key}");
                    current.UpdateMetrics();
                    current = current.Parent;
                }

                current = this;
                while (current != null)
                {
                    Console.WriteLine($"TreeNode.RemoveValues: Rebalancing node {current.Key}");
                    current.Rebalance();
                    current = current.Parent;
                }
            }

            #endregion
        }

        public TreeNode Root { get; private set; }

        #region Main Func

        public void Add(TKey key, Sales value)
        {
            Console.WriteLine($"{key} Tree.Add: Starting add - Key: {key}, Value: {value}");
            if (Root == null)
            {
                Console.WriteLine($"{key} Tree.Add: Root is null, creating new root node");
                Root = new TreeNode(key, this);
            }
            else
            {
                Console.WriteLine($"{key} Tree.Add: Root exists, delegating to root node");
            }
            Root.Add(key, value);
            Console.WriteLine($"{key} Tree.Add: Completed add - Key: {key}");
        }

        public void CreateKey(TKey key)
        {
            Console.WriteLine($"{key} Tree.CreateKey: Creating key");

            if (Root == null)
            {
                Console.WriteLine($"{key} Tree.CreateKey: Root is null, creating new root node");
                Root = new TreeNode(key, this);
            }
            else
            {
                Console.WriteLine($"{key} Tree.CreateKey: Root exists, adding key through root node");
                Root.Add(key, default);
            }
            Console.WriteLine($"{key} Tree.CreateKey: Completed creating key");
        }

        private TreeNode FindNode(TreeNode node, TKey key)
        {
            Console.WriteLine($"Tree.FindNode: Searching for key {key} in node {(node == null ? "null" : node.Key.ToString())}");
            if (node == null)
            {
                Console.WriteLine($"Tree.FindNode: Node is null, key {key} not found");
                return null;
            }
            int cmp = key.CompareTo(node.Key);
            Console.WriteLine($"Tree.FindNode: Comparison result: {cmp}");
            if (cmp == 0)
            {
                Console.WriteLine($"Tree.FindNode: Key {key} found in node {node.Key}");
                return node;
            }
            return cmp < 0 ? FindNode(node.Left, key) : FindNode(node.Right, key);
        }

        public bool Contains(TKey key)
        {
            Console.WriteLine($"Tree.Contains: Starting search for key {key}");
            bool result = Root?.Contains(key) ?? false;
            Console.WriteLine($"Tree.Contains: Key {key} {(result ? "found" : "not found")} in tree");
            return result;
        }

        public bool Remove(TKey key)
        {
            Console.WriteLine($"{key} Tree.Remove: Starting removal");
            bool result = Root?.Remove(key) ?? false;
            Console.WriteLine($"{key} Tree.Remove: {(result ? "successfully removed" : "not found")}");
            return result;
        }

        #endregion

        #region Control and Display Func

        public void Clear()
        {
            Console.WriteLine($"Tree.Clear: Starting tree clear");
            Root?.Clear();
            Root = null;
            Console.WriteLine($"Tree.Clear: Tree cleared");
        }

        public int Count
        {
            get
            {
                int count = Root?.Count ?? 0;
                Console.WriteLine($"Tree.Count: Returning count: {count}");
                return count;
            }
        }

        public void CopyTo(Sales[] array, int arrayIndex)
        {
            Console.WriteLine($"Tree.CopyTo: Starting copy to array, starting index: {arrayIndex}");
            if (array == null)
            {
                Console.WriteLine($"Tree.CopyTo: Array is null, throwing exception");
                throw new ArgumentNullException(nameof(array));
            }
            if (arrayIndex < 0)
            {
                Console.WriteLine($"Tree.CopyTo: Array index {arrayIndex} is negative, throwing exception");
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }
            if (array.Length - arrayIndex < Count)
            {
                Console.WriteLine($"Tree.CopyTo: Insufficient array size. Required: {Count}, Available: {array.Length - arrayIndex}");
                throw new ArgumentException("Insufficient array size");
            }
            Root?.CopyTo(array, arrayIndex);
            Console.WriteLine($"Tree.CopyTo: Copy completed");
        }

        public int IndexOf(TKey key)
        {
            Console.WriteLine($"{key} Tree.IndexOf: Finding index");
            int result = Root?.IndexOf(key) ?? -1;
            Console.WriteLine($"{key} Tree.IndexOf: found at index: {result}");
            return result;
        }

        // Индексатор для безопасного получения вектора
        public Vector<Sales> this[TKey key]
        {
            get
            {
                Console.WriteLine($"Tree.Indexer: Accessing values for key {key}");
                if (Root == null)
                {
                    Console.WriteLine($"Tree.Indexer: Tree is empty, throwing exception");
                    throw new InvalidOperationException("Tree is empty. Cannot access key.");
                }
                Vector<Sales> result = GetValuesByKey(Root, key);
                Console.WriteLine($"Tree.Indexer: Returning vector with {result.Count} values for key {key}");
                return result;
            }
        }

        private Vector<Sales> GetValuesByKey(TreeNode node, TKey key)
        {
            Console.WriteLine($"Tree.GetValuesByKey: Getting values for key {key} from node {(node == null ? "null" : node.Key.ToString())}");
            if (node == null)
            {
                Console.WriteLine($"Tree.GetValuesByKey: Node is null, returning empty vector");
                return new Vector<Sales>();
            }
            int cmp = key.CompareTo(node.Key);
            Console.WriteLine($"Tree.GetValuesByKey: Comparison result: {cmp}");
            if (cmp == 0)
            {
                Console.WriteLine($"Tree.GetValuesByKey: Key matched, returning vector with {node.Values.Count} values");
                return node.Values;
            }
            return cmp < 0 ? GetValuesByKey(node.Left, key) : GetValuesByKey(node.Right, key);
        }

        public string Display()
        {
            Console.WriteLine($"Tree.Display: Generating tree display");
            string result = DisplayRecur(Root);
            Console.WriteLine($"Tree.Display: Display generated, length: {result.Length}");
            return result;
        }

        private string DisplayRecur(TreeNode node, string indent = "", bool isLast = true)
        {
            if (node == null)
            {
                Console.WriteLine($"Tree.DisplayRecur: Node is null, returning empty string");
                return string.Empty;
            }

            Console.WriteLine($"Tree.DisplayRecur: Processing node {node.Key}, indent: '{indent}', isLast: {isLast}");
            var sb = new StringBuilder();

            sb.Append(DisplayRecur(node.Right, indent + (isLast ? "    " : "│   "), false));
            sb.Append(indent)
              .Append(isLast ? "└── " : "├── ")
              .Append(node.Key)
              .Append(" (Count=")
              .Append(node.Values.Count)
              .AppendLine(")");

            for (int i = 0; i < node.Values.Count; i++)
            {
                sb.Append(indent)
                  .Append(isLast ? "    " : "│   ")
                  .Append("│   ")
                  .AppendLine(node.Values[i].ToString());
            }

            sb.Append(DisplayRecur(node.Left, indent + (isLast ? "    " : "│   "), true));

            Console.WriteLine($"Tree.DisplayRecur: Completed processing node {node.Key}");
            return sb.ToString();
        }

        #endregion

        #region Additional Remove Methods

        /// <summary>
        /// Удаляет строго указанный объект Sales из узла дерева по заданной дате (key).
        /// Возвращает true, если элемент был найден и удалён.
        /// </summary>
        public bool RemoveExactSalesAtDate(TKey dateKey, Sales sale)
        {
            Console.WriteLine($"Tree.RemoveExactSalesAtDate: Starting removal - DateKey: {dateKey}, Sale: {sale}");
            var node = FindNode(Root, dateKey);
            if (node == null)
            {
                Console.WriteLine($"Tree.RemoveExactSalesAtDate: Node for date {dateKey} not found");
                return false;
            }

            Console.WriteLine($"Tree.RemoveExactSalesAtDate: Node found, checking values");
            int before = node.Values.Count;
            Console.WriteLine($"Tree.RemoveExactSalesAtDate: Before removal - Values count: {before}");

            node.RemoveValues(s => s.Equals(sale));

            int after = node.Values.Count;
            bool result = before != after;
            Console.WriteLine($"Tree.RemoveExactSalesAtDate: After removal - Values count: {after}, removed: {result}");

            return result;
        }

        public void RemoveEmptyNodes()
        {
            Console.WriteLine($"Tree.RemoveEmptyNodes: Starting removal of empty nodes");
            Root = RemoveEmptyNodesRecursive(Root);
            Console.WriteLine($"Tree.RemoveEmptyNodes: Completed removal of empty nodes");
        }

        private TreeNode RemoveEmptyNodesRecursive(TreeNode node)
        {
            Console.WriteLine($"Tree.RemoveEmptyNodesRecursive: Processing node {(node == null ? "null" : node.Key.ToString())}");
            if (node == null)
            {
                Console.WriteLine($"Tree.RemoveEmptyNodesRecursive: Node is null, returning null");
                return null;
            }

            // Рекурсивно очищаем потомков
            Console.WriteLine($"Tree.RemoveEmptyNodesRecursive: Processing left child");
            node.Left = RemoveEmptyNodesRecursive(node.Left);
            if (node.Left != null)
            {
                node.Left.Parent = node;
                Console.WriteLine($"Tree.RemoveEmptyNodesRecursive: Left child parent set to current node");
            }

            Console.WriteLine($"Tree.RemoveEmptyNodesRecursive: Processing right child");
            node.Right = RemoveEmptyNodesRecursive(node.Right);
            if (node.Right != null)
            {
                node.Right.Parent = node;
                Console.WriteLine($"Tree.RemoveEmptyNodesRecursive: Right child parent set to current node");
            }

            // Если в узле нет записей, удаляем его
            if (node.Values.Count == 0)
            {
                Console.WriteLine($"Tree.RemoveEmptyNodesRecursive: Node {node.Key} has no values, removing it");
                Console.WriteLine($"Tree.RemoveEmptyNodesRecursive: Left={(node.Left == null ? "null" : "exists")}, Right={(node.Right == null ? "null" : "exists")}");

                // Один потомок или ни одного
                if (node.Left == null)
                {
                    Console.WriteLine($"Tree.RemoveEmptyNodesRecursive: No left child, returning right child");
                    return node.Right;
                }
                if (node.Right == null)
                {
                    Console.WriteLine($"Tree.RemoveEmptyNodesRecursive: No right child, returning left child");
                    return node.Left;
                }

                // Оба потомка существуют: выбираем правое поддерево как новый корень
                Console.WriteLine($"Tree.RemoveEmptyNodesRecursive: Both children exist, using right subtree as new root");
                var newRoot = node.Right;
                // Находим минимальный элемент в правом поддереве
                var min = newRoot;
                while (min.Left != null)
                {
                    Console.WriteLine($"Tree.RemoveEmptyNodesRecursive: Moving to leftmost node in right subtree: {min.Key}");
                    min = min.Left;
                }
                Console.WriteLine($"Tree.RemoveEmptyNodesRecursive: Minimum node found: {min.Key}");

                // Прикрепляем левое поддерево к минимальному узлу
                min.Left = node.Left;
                node.Left.Parent = min;
                Console.WriteLine($"Tree.RemoveEmptyNodesRecursive: Attached left subtree to minimum node");

                return newRoot;
            }

            Console.WriteLine($"Tree.RemoveEmptyNodesRecursive: Node {node.Key} has values, keeping it");
            return node;
        }

        /// <summary>
        /// Удаляет продажи по указанному article во всех узлах дерева.
        /// Возвращает общее число удалённых записей.
        /// </summary>
        public int RemoveSalesByArticleAcrossAllDates(Article article)
        {
            Console.WriteLine($"Tree.RemoveSalesByArticleAcrossAllDates: Starting removal for article {article}");
            int result = RemoveSalesRecursive(Root, article);
            Console.WriteLine($"Tree.RemoveSalesByArticleAcrossAllDates: Completed removal, total removed: {result}");
            return result;
        }

        private int RemoveSalesRecursive(TreeNode node, Article article)
        {
            Console.WriteLine($"Tree.RemoveSalesRecursive: Processing node {(node == null ? "null" : node.Key.ToString())} for article {article}");
            if (node == null)
            {
                Console.WriteLine($"Tree.RemoveSalesRecursive: Node is null, returning 0");
                return 0;
            }

            int removedLeft = RemoveSalesRecursive(node.Left, article);
            Console.WriteLine($"Tree.RemoveSalesRecursive: Removed {removedLeft} from left subtree of node {node.Key}");

            int before = node.Values.Count;
            Console.WriteLine($"Tree.RemoveSalesRecursive: Before removal - Node {node.Key} has {before} values");

            node.RemoveValues(s => s.Article.Equals(article));

            int after = node.Values.Count;
            int removedHere = before - after;
            Console.WriteLine($"Tree.RemoveSalesRecursive: Removed {removedHere} from current node {node.Key}");

            int removedRight = RemoveSalesRecursive(node.Right, article);
            Console.WriteLine($"Tree.RemoveSalesRecursive: Removed {removedRight} from right subtree of node {node.Key}");

            int total = removedLeft + removedHere + removedRight;
            Console.WriteLine($"Tree.RemoveSalesRecursive: Total removed from node {node.Key} and subtrees: {total}");

            return total;
        }

        #endregion
    }
}