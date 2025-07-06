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
                Key = key;
                Tree = tree;
                Height = 1;
                Values = new Vector<Sales>();
                Count = 0;
            }

            #region Main Func

            public void Add(TKey key, Sales value)
            {
                int cmp = key.CompareTo(Key);
                if (cmp == 0)
                {
                    Values.Add(value);
                    UpdateMetrics();
                    return;
                }
                if (cmp < 0)
                {
                    if (Left == null)
                        Left = CreateNode(key, value);
                    else
                        Left.Add(key, value);
                }
                else
                {
                    if (Right == null)
                        Right = CreateNode(key, value);
                    else
                        Right.Add(key, value);
                }
                Rebalance();
            }

            private TreeNode CreateNode(TKey key, Sales value)
            {
                var node = new TreeNode(key, Tree) { Parent = this };
                node.Values.Add(value);
                node.UpdateMetrics();
                return node;
            }

            public void Clear()
            {
                Left?.Clear();
                Right?.Clear();
                Left = Right = null;
                Values = new Vector<Sales>();
                Count = 0;
                Height = 1;
            }

            public bool Contains(TKey key)
            {
                int cmp = key.CompareTo(Key);
                if (cmp == 0) return true;
                return cmp < 0 ? Left?.Contains(key) ?? false : Right?.Contains(key) ?? false;
            }

            public bool Remove(TKey key)
            {
                int cmp = key.CompareTo(Key);
                if (cmp < 0)
                    return Left?.Remove(key) ?? false;
                if (cmp > 0)
                    return Right?.Remove(key) ?? false;

                if (Left == null || Right == null)
                {
                    var child = Left ?? Right;
                    if (Parent == null)
                    {
                        Tree.Root = child;
                        if (child != null) child.Parent = null;
                    }
                    else if (Parent.Left == this)
                        Parent.Left = child;
                    else
                        Parent.Right = child;
                    if (child != null) child.Parent = Parent;
                }
                else
                {
                    var pred = Left;
                    while (pred.Right != null) pred = pred.Right;
                    Key = pred.Key;
                    Values = pred.Values;
                    pred.Remove(pred.Key);
                }

                Parent?.Rebalance();
                return true;
            }

            #endregion

            #region Balancing and Control Func

            public void CopyTo(Sales[] array, int index)
            {
                Left?.CopyTo(array, index);
                if (Left != null) index += Left.Count;
                for (int i = 0; i < Values.Count; i++) array[index++] = Values[i];
                Right?.CopyTo(array, index);
            }

            public int IndexOf(TKey key)
            {
                int cmp = key.CompareTo(Key);
                if (cmp == 0)
                    return Left?.Count ?? 0;
                if (cmp < 0)
                    return Left?.IndexOf(key) ?? -1;
                var baseIdx = (Left?.Count ?? 0) + Values.Count;
                int idx = Right?.IndexOf(key) ?? -1;
                return idx < 0 ? -1 : baseIdx + idx;
            }

            private void Rebalance()
            {
                UpdateMetrics();
                int balance = (Left?.Height ?? 0) - (Right?.Height ?? 0);
                if (balance > 1)
                {
                    if ((Left.Left?.Height ?? 0) < (Left.Right?.Height ?? 0)) Left.RotateLeft();
                    RotateRight();
                }
                else if (balance < -1)
                {
                    if ((Right.Right?.Height ?? 0) < (Right.Left?.Height ?? 0)) Right.RotateRight();
                    RotateLeft();
                }
            }

            private void UpdateMetrics()
            {
                int lh = Left?.Height ?? 0;
                int rh = Right?.Height ?? 0;
                Height = Math.Max(lh, rh) + 1;
                Count = (Left?.Count ?? 0) + (Right?.Count ?? 0) + Values.Count;
            }

            private void RotateLeft()
            {
                var pivot = Right;
                ReplaceInParent(pivot);
                Right = pivot.Left;
                if (pivot.Left != null) pivot.Left.Parent = this;
                pivot.Left = this;
                pivot.Parent = this.Parent;
                this.Parent = pivot;
                UpdateMetrics();
                pivot.UpdateMetrics();
            }

            private void RotateRight()
            {
                var pivot = Left;
                ReplaceInParent(pivot);
                Left = pivot.Right;
                if (pivot.Right != null) pivot.Right.Parent = this;
                pivot.Right = this;
                pivot.Parent = this.Parent;
                this.Parent = pivot;
                UpdateMetrics();
                pivot.UpdateMetrics();
            }

            private void ReplaceInParent(TreeNode node)
            {
                node.Parent = Parent;
                if (Parent == null) Tree.Root = node;
                else if (Parent.Left == this) Parent.Left = node;
                else Parent.Right = node;
            }

            #endregion

            #region Additional Remove Func

            internal void RemoveValues(Predicate<Sales> predicate)
            {
                var newValues = new Vector<Sales>();
                for (int i = 0; i < Values.Count; i++)
                {
                    if (!predicate(Values[i])) newValues.Add(Values[i]);
                }
                Values = newValues;
                var current = this;
                while (current != null)
                {
                    current.UpdateMetrics();
                    current = current.Parent;
                }
                current = this;
                while (current != null)
                {
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
            if (Root == null) Root = new TreeNode(key, this);
            Root.Add(key, value);
        }

        public void CreateKey(TKey key)
        {
            if (Root == null)
            {
                Root = new TreeNode(key, this);
            }
            else
            {
                Root.Add(key, default); // Добавляет ключ без реального значения
            }
        }

        private TreeNode FindNode(TreeNode node, TKey key)
        {
            if (node == null) return null;
            int cmp = key.CompareTo(node.Key);
            if (cmp == 0) return node;
            return cmp < 0 ? FindNode(node.Left, key) : FindNode(node.Right, key);
        }

        public bool Contains(TKey key) => Root?.Contains(key) ?? false;

        public bool Remove(TKey key) => Root?.Remove(key) ?? false;

        #endregion

        #region Control and Display Func

        public void Clear()
        {
            Root?.Clear();
            Root = null;
        }

        public int Count => Root?.Count ?? 0;

        public void CopyTo(Sales[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length - arrayIndex < Count) throw new ArgumentException("Insufficient array size");
            Root?.CopyTo(array, arrayIndex);
        }

        public int IndexOf(TKey key) => Root?.IndexOf(key) ?? -1;

        // Индексатор для безопасного получения вектора
        public Vector<Sales> this[TKey key]
        {
            get
            {
                if (Root == null) throw new InvalidOperationException("Tree is empty. Cannot access key.");
                return GetValuesByKey(Root, key);
            }
        }

        private Vector<Sales> GetValuesByKey(TreeNode node, TKey key)
        {
            if (node == null) return new Vector<Sales>();
            int cmp = key.CompareTo(node.Key);
            if (cmp == 0) return node.Values;
            return cmp < 0 ? GetValuesByKey(node.Left, key) : GetValuesByKey(node.Right, key);
        }

        public string Display() => DisplayRecur(Root);

        private string DisplayRecur(TreeNode node, string indent = "", bool isLast = true)
        {
            if (node == null) return string.Empty;
            var sb = new StringBuilder();
            sb.Append(DisplayRecur(node.Right, indent + (isLast ? "    " : "│   "), false));
            sb.Append(indent)
              .Append(isLast ? "└── " : "├── ")
              .Append(node.Key)
              .Append(" (Count=")
              .Append(node.Values.Count)
              .AppendLine(")");
            for (int i = 0; i < node.Values.Count; i++)
                sb.Append(indent)
                  .Append(isLast ? "    " : "│   ")
                  .Append("│   ")
                  .AppendLine(node.Values[i].ToString());
            sb.Append(DisplayRecur(node.Left, indent + (isLast ? "    " : "│   "), true));
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
            var node = FindNode(Root, dateKey);
            if (node == null)
                return false;

            int before = node.Values.Count;

            node.RemoveValues(s => s.Equals(sale));

            int after = node.Values.Count;

            return before != after;
        }

        public void RemoveEmptyNodes()
        {
            Root = RemoveEmptyNodesRecursive(Root);
        }

        private TreeNode RemoveEmptyNodesRecursive(TreeNode node)
        {
            if (node == null) return null;

            // Рекурсивно очищаем потомков
            node.Left = RemoveEmptyNodesRecursive(node.Left);
            if (node.Left != null) node.Left.Parent = node;
            node.Right = RemoveEmptyNodesRecursive(node.Right);
            if (node.Right != null) node.Right.Parent = node;

            // Если в узле нет записей, удаляем его
            if (node.Values.Count == 0)
            {
                // Один потомок или ни одного
                if (node.Left == null) return node.Right;
                if (node.Right == null) return node.Left;

                // Оба потомка существуют: выбираем правое поддерево как новый корень
                var newRoot = node.Right;
                // Находим минимальный элемент в правом поддереве
                var min = newRoot;
                while (min.Left != null)
                    min = min.Left;
                // Прикрепляем левое поддерево к минимальному узлу
                min.Left = node.Left;
                node.Left.Parent = min;
                return newRoot;
            }

            return node;
        }

        /// <summary>
        /// Удаляет продажи по указанному article во всех узлах дерева.
        /// Возвращает общее число удалённых записей.
        /// </summary>
        public int RemoveSalesByArticleAcrossAllDates(Article article)
        {
            return RemoveSalesRecursive(Root, article);
        }

        private int RemoveSalesRecursive(TreeNode node, Article article)
        {
            if (node == null) return 0;
            int removedLeft = RemoveSalesRecursive(node.Left, article);
            int before = node.Values.Count;
            node.RemoveValues(s => s.Article.Equals(article));
            int after = node.Values.Count;
            int removedHere = before - after;
            int removedRight = RemoveSalesRecursive(node.Right, article);
            return removedLeft + removedHere + removedRight;
        }

        #endregion
    }
}
