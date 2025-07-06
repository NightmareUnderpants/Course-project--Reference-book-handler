using CoreLogic.Structures;
using CoreLogic.Vector;
using System;
using System.Diagnostics;

namespace CoreLogic.Classes
{
    public class Search
    {
        /// <summary>
        /// Ищет товар и список продаж по артикулу.
        /// Возвращает true, если хотя бы в хеш-таблице он найден.
        /// </summary>
        public static bool SearchByArticle(in HashTable hashTable, in Tree<Article> tree,
            Article article, out Goods goods, out Vector<Sales> salesVector)
        {
            // Поиск в хеш-таблице
            if (!hashTable.Find(article, out var foundGoods, out int steps))
            {
                Debug.WriteLine($"Warning: Article: {article} not found in HashTable (after {steps} probes)");
                goods = default;
                salesVector = default;
                return false;
            }

            // Заполняем выходной параметр
            goods = foundGoods;

            // Поиск в дереве
            if (!TrySearchArticleInTree(tree, article, out var articleVector))
            {
                Debug.WriteLine($"Warning: Article: {article} not found in Tree");
                salesVector = new Vector<Sales>();  // или default, по вашему усмотрению
            }
            else
            {
                salesVector = articleVector;
            }

            return true;
        }

        public static void SearchByDate(in Tree<Date> tree, Date date, out Vector<Sales> salesVector)
        {
            salesVector = new Vector<Sales>();

            try
            {
                int indexSales = tree.IndexOf(date);
                if (indexSales == -1)
                {
                    salesVector = new Vector<Sales>();
                    return;
                }

                salesVector = tree[date];
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при поиске: {ex.Message}", "Ошибка");
                salesVector = new Vector<Sales>();
            }
        }


        private static bool TrySearchArticleInTree(Tree<Article> tree, Article article, out Vector<Sales> vectorArticle)
        {
            // Получаем индекс; IndexOf сейчас возвращает -1, если нет
            int idx = tree.IndexOf(article);
            if (idx < 0)
            {
                vectorArticle = default;
                return false;
            }

            // Обращаемся по ключу — здесь бросится, только если дерево пустое
            vectorArticle = tree[article];
            return true;
        }
    }
}
