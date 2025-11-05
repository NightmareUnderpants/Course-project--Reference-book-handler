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
            public Vector<CircularLinkedList<Sales>.Node> NodeReferences { get; private set; }
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
                NodeReferences = new Vector<CircularLinkedList<Sales>.Node>();
                Count = 0;
            }

            #region Основные методы (работа со ссылками)

            public void Add(TKey key, CircularLinkedList<Sales>.Node nodeRef)
            {
                int cmp = key.CompareTo(Key);
                if (cmp == 0)
                {
                    // Добавляем ссылку только если она не null
                    if (nodeRef != null)
                    {
                        NodeReferences.Add(nodeRef);
                        UpdateMetrics();
                    }
                    return;
                }

                if (cmp < 0)
                {
                    if (Left == null)
                    {
                        Left = CreateNode(key, nodeRef);
                    }
                    else
                    {
                        Left.Add(key, nodeRef);
                    }
                }
                else
                {
                    if (Right == null)
                    {
                        Right = CreateNode(key, nodeRef);
                    }
                    else
                    {
                        Right.Add(key, nodeRef);
                    }
                }

                Rebalance();
            }

            private TreeNode CreateNode(TKey key, CircularLinkedList<Sales>.Node nodeRef)
            {
                var node = new TreeNode(key, Tree) { Parent = this };
                // Добавляем ссылку только если она не null
                if (nodeRef != null)
                {
                    node.NodeReferences.Add(nodeRef);
                }
                node.UpdateMetrics();
                return node;
            }

            public bool Contains(TKey key)
            {
                int cmp = key.CompareTo(Key);
                if (cmp == 0)
                    return true;

                return cmp < 0 ? Left?.Contains(key) ?? false : Right?.Contains(key) ?? false;
            }

            public bool Remove(TKey key)
            {
                int cmp = key.CompareTo(Key);
                if (cmp < 0)
                    return Left?.Remove(key) ?? false;

                if (cmp > 0)
                    return Right?.Remove(key) ?? false;

                // Удаляем только из дерева, не трогая сами объекты в списке
                if (Left == null || Right == null)
                {
                    var child = Left ?? Right;
                    if (Parent == null)
                    {
                        Tree.Root = child;
                    }
                    else if (Parent.Left == this)
                    {
                        Parent.Left = child;
                    }
                    else
                    {
                        Parent.Right = child;
                    }
                    if (child != null) child.Parent = Parent;
                }
                else
                {
                    var pred = Left;
                    while (pred.Right != null) pred = pred.Right;

                    TKey predKey = pred.Key;

                    pred.Remove(pred.Key);

                    this.Key = predKey;
                }

                Parent?.Rebalance();
                return true;
            }

            #endregion

            #region Вспомогательные методы для работы со ссылками

            internal void RemoveNodeReferences(Predicate<CircularLinkedList<Sales>.Node> predicate)
            {
                var newReferences = new Vector<CircularLinkedList<Sales>.Node>();
                for (int i = 0; i < NodeReferences.Count; i++)
                {
                    if (!predicate(NodeReferences[i]))
                    {
                        newReferences.Add(NodeReferences[i]);
                    }
                }
                NodeReferences = newReferences;
                UpdateMetricsAndPropagate();
            }

            private void UpdateMetricsAndPropagate()
            {
                UpdateMetrics();
                var current = this.Parent;
                while (current != null)
                {
                    current.UpdateMetrics();
                    current = current.Parent;
                }
            }

            #endregion

            #region Балансировка и служебные методы

            private void Rebalance()
            {
                UpdateMetrics();
                int balance = (Left?.Height ?? 0) - (Right?.Height ?? 0);

                if (balance > 1)
                {
                    if ((Left.Left?.Height ?? 0) < (Left.Right?.Height ?? 0))
                    {
                        Left.RotateLeft();
                    }
                    RotateRight();
                }
                else if (balance < -1)
                {
                    if ((Right.Right?.Height ?? 0) < (Right.Left?.Height ?? 0))
                    {
                        Right.RotateRight();
                    }
                    RotateLeft();
                }
            }

            private void UpdateMetrics()
            {
                int lh = Left?.Height ?? 0;
                int rh = Right?.Height ?? 0;
                Height = Math.Max(lh, rh) + 1;
                Count = (Left?.Count ?? 0) + (Right?.Count ?? 0) + NodeReferences.Count;
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
                if (Parent == null)
                {
                    Tree.Root = node;
                }
                else if (Parent.Left == this)
                {
                    Parent.Left = node;
                }
                else
                {
                    Parent.Right = node;
                }
            }

            #endregion
        }

        public TreeNode Root { get; private set; }

        #region Публичный интерфейс (работа со ссылками)

        /// <summary>
        /// Добавляет ССЫЛКУ на узел из общего списка продаж
        /// </summary>
        public void Add(TKey key, CircularLinkedList<Sales>.Node nodeRef)
        {
            if (nodeRef == null)
            {
                // Если ссылка null, просто создаем узел без значений
                CreateKey(key);
                return;
            }

            if (Root == null)
            {
                // Создаем корневой узел и сразу добавляем в него ссылку
                Root = new TreeNode(key, this);
                Root.NodeReferences.Add(nodeRef);
                return;
            }

            Root.Add(key, nodeRef);
        }

        /// <summary>
        /// Создаёт ключ (узел) без значений
        /// </summary>
        public void CreateKey(TKey key)
        {
            if (Root == null)
            {
                Root = new TreeNode(key, this);
                return;
            }

            // Ищем узел с таким ключом
            var node = FindNode(Root, key);
            if (node == null)
            {
                // Если узел не найден, добавляем его
                Root.Add(key, null);
            }
            // Если узел уже существует, ничего не делаем
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

        public Vector<CircularLinkedList<Sales>.Node> this[TKey key]
        {
            get
            {
                if (Root == null)
                    return new Vector<CircularLinkedList<Sales>.Node>(); // Возвращаем пустой вектор вместо исключения

                return GetValuesByKey(Root, key);
            }
        }

        private Vector<CircularLinkedList<Sales>.Node> GetValuesByKey(TreeNode node, TKey key)
        {
            if (node == null)
                return new Vector<CircularLinkedList<Sales>.Node>();

            int cmp = key.CompareTo(node.Key);

            if (cmp == 0)
                return node.NodeReferences;

            return cmp < 0 ?
                GetValuesByKey(node.Left, key) :
                GetValuesByKey(node.Right, key);
        }

        #endregion

        #region Методы для получения данных

        /// <summary>
        /// Возвращает ВСЕ ССЫЛКИ на узлы продаж по указанной дате
        /// </summary>
        public Vector<CircularLinkedList<Sales>.Node> GetNodeReferences(TKey key)
        {
            if (Root == null)
                return new Vector<CircularLinkedList<Sales>.Node>(); // Возвращаем пустой вектор вместо исключения

            return GetNodeReferencesByKey(Root, key);
        }

        private Vector<CircularLinkedList<Sales>.Node> GetNodeReferencesByKey(TreeNode node, TKey key)
        {
            if (node == null)
                return new Vector<CircularLinkedList<Sales>.Node>();

            int cmp = key.CompareTo(node.Key);
            if (cmp == 0)
                return node.NodeReferences;

            return cmp < 0 ?
                GetNodeReferencesByKey(node.Left, key) :
                GetNodeReferencesByKey(node.Right, key);
        }

        /// <summary>
        /// Возвращает ВЕКТОР САМИХ ОБЪЕКТОВ Sales для обратной совместимости
        /// </summary>
        public Vector<Sales> GetSalesValues(TKey key)
        {
            var references = GetNodeReferences(key);
            var result = new Vector<Sales>();

            for (int i = 0; i < references.Count; i++)
            {
                if (references[i] != null)
                    result.Add(references[i].Value);
            }

            return result;
        }

        public string Display()
        {
            return DisplayRecur(Root);
        }

        private string DisplayRecur(TreeNode node, string indent = "", bool isLast = true)
        {
            if (node == null) return string.Empty;

            var sb = new StringBuilder();

            // Сначала обрабатываем правое поддерево
            sb.Append(DisplayRecur(node.Right, indent + (isLast ? "    " : "│   "), false));

            // Выводим текущий узел
            sb.Append(indent)
              .Append(isLast ? "└── " : "├── ")
              .Append(node.Key)
              .Append(" (Refs=")
              .Append(node.NodeReferences.Count)
              .AppendLine(")");

            // Выводим все Sales в этом узле
            for (int i = 0; i < node.NodeReferences.Count; i++)
            {
                if (node.NodeReferences[i] != null)
                {
                    var sale = node.NodeReferences[i].Value;
                    // Форматируем строку продажи с дополнительным отступом
                    sb.Append(indent)
                      .Append(isLast ? "    " : "│   ") // Отступ под текущим узлом
                      .Append("│   ")                   // Дополнительный отступ для содержимого
                      .AppendLine(sale.ToString());     // Предполагается, что Sale имеет хороший ToString()
                }
            }

            // Затем обрабатываем левое поддерево
            sb.Append(DisplayRecur(node.Left, indent + (isLast ? "    " : "│   "), true));

            return sb.ToString();
        }

        #endregion

        #region Дополнительные методы удаления

        /// <summary>
        /// Удаляет ССЫЛКУ на конкретный узел продажи по дате
        /// </summary>
        public bool RemoveExactNodeReferenceAtDate(TKey dateKey, CircularLinkedList<Sales>.Node nodeRef)
        {
            var node = FindNode(Root, dateKey);
            if (node == null) return false;

            int before = node.NodeReferences.Count;
            node.RemoveNodeReferences(refNode => refNode == nodeRef);
            return before != node.NodeReferences.Count;
        }

        /// <summary>
        /// Удаляет ВСЕ ССЫЛКИ, связанные с указанным артикулом
        /// </summary>
        public int RemoveNodeReferencesByArticle(Article article)
        {
            return RemoveNodeReferencesRecursive(Root, article);
        }

        private int RemoveNodeReferencesRecursive(TreeNode node, Article article)
        {
            if (node == null) return 0;

            int removedLeft = RemoveNodeReferencesRecursive(node.Left, article);
            int before = node.NodeReferences.Count;

            node.RemoveNodeReferences(refNode =>
                refNode != null && refNode.Value.Article.Equals(article));

            int removedHere = before - node.NodeReferences.Count;
            int removedRight = RemoveNodeReferencesRecursive(node.Right, article);

            return removedLeft + removedHere + removedRight;
        }

        public void RemoveEmptyNodes()
        {
            Root = RemoveEmptyNodesRecursive(Root);
        }

        private TreeNode RemoveEmptyNodesRecursive(TreeNode node)
        {
            if (node == null) return null;

            node.Left = RemoveEmptyNodesRecursive(node.Left);
            if (node.Left != null) node.Left.Parent = node;

            node.Right = RemoveEmptyNodesRecursive(node.Right);
            if (node.Right != null) node.Right.Parent = node;

            // Если нет ссылок на продажи - удаляем узел
            if (node.NodeReferences.Count == 0)
            {
                if (node.Left == null) return node.Right;
                if (node.Right == null) return node.Left;

                // Оба потомка существуют
                var newRoot = node.Right;
                var min = newRoot;
                while (min.Left != null) min = min.Left;

                min.Left = node.Left;
                node.Left.Parent = min;
                return newRoot;
            }

            return node;
        }

        #endregion
    }
}