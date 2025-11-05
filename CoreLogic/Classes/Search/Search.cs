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
        /// Возвращает true, если товар найден в хеш-таблице.
        /// </summary>
        public static bool SearchByArticle(
            HashTable hashTable,
            Tree<Article> articleTree,
            Article article,
            out Goods goods,
            out Vector<Sales> salesVector)
        {
            goods = default;
            salesVector = new Vector<Sales>();

            // 1. Поиск товара в ХЕШ-ТАБЛИЦЕ (получаем ССЫЛКУ на узел)
            if (!hashTable.Find(article, out CircularLinkedList<Goods>.Node goodsNodeRef))
            {
                Debug.WriteLine($"Warning: Article {article} not found in HashTable");
                return false;
            }

            // 2. Получаем сам объект Goods из ссылки
            goods = goodsNodeRef.Value;

            // 3. Поиск продаж в ДЕРЕВЕ СТАТЕЙ
            var salesNodeReferences = articleTree.GetNodeReferences(article);

            // 4. Конвертируем ссылки в вектор значений Sales
            salesVector = new Vector<Sales>();
            for (int i = 0; i < salesNodeReferences.Count; i++)
            {
                if (salesNodeReferences[i] != null)
                {
                    salesVector.Add(salesNodeReferences[i].Value);
                }
            }

            return true;
        }

        /// <summary>
        /// Ищет все продажи по дате в дереве дат.
        /// </summary>
        public static void SearchByDate(
            Tree<Date> dateTree,
            Date date,
            out Vector<Sales> salesVector)
        {
            salesVector = new Vector<Sales>();

            try
            {
                // 1. Получаем ССЫЛКИ на узлы продаж по дате
                var salesNodeReferences = dateTree.GetNodeReferences(date);

                // 2. Конвертируем ссылки в вектор значений Sales
                for (int i = 0; i < salesNodeReferences.Count; i++)
                {
                    if (salesNodeReferences[i] != null)
                    {
                        salesVector.Add(salesNodeReferences[i].Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при поиске по дате {date}: {ex.Message}");
                salesVector = new Vector<Sales>();
            }
        }
    }
}