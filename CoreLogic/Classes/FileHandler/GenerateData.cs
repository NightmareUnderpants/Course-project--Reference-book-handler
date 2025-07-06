using CoreLogic.Structures;
using CoreLogic.Vector;
using System;

namespace CoreLogic.FileHandler.GenerateData
{
    public class GenerateData
    {
        /// <summary>
        /// Генерирует коллекцию тестовых данных о продажах.
        /// </summary>
        /// <param name="count">
        /// Количество генерируемых записей о продажах.
        /// Должно быть положительным числом (больше 0).
        /// </param>
        /// <param name="sales">
        /// Исходная коллекция продаж, на основе которой генерируются новые данные.
        /// Может использоваться для сохранения уже существующих записей.
        /// </param>
        /// <returns>
        /// Новая коллекция <see cref="Vector{Sales}"/>, содержащая:
        /// <list type="bullet">
        ///     <item><description>Все элементы из исходной коллекции <paramref name="sales"/></description></item>
        ///     <item><description>Добавленные сгенерированные записи (в количестве <paramref name="count"/>)</description></item>
        /// </list>
        /// </returns>
        public static Vector<Sales> GenerateSales(int count, Vector<Sales> sales)
        {
            int size = sales.Count;
            Random rand = new Random();
            Vector<Sales> newSales = new Vector<Sales>(count);

            for (int i = 0; i < count; i++)
            {
                // full rand generate by file
                //int     Sales_article = sales[rand.Next(size)].article;
                //int     Sales_count   = sales[rand.Next(size)].count;
                //string  Sales_cashier = sales[rand.Next(size)].cashier;
                //Date    Sales_date    = sales[rand.Next(size)].date;

                // rand generation by int rand and by file
                int Sales_article_id = rand.Next(10000, 99999);
                Article.Category Sales_article_category = (Article.Category)rand.Next(
                    0,
                    Enum.GetValues(typeof(Article.Category)).Length
                );

                int Sales_count = rand.Next(1, 100);
                string Sales_cashier = sales[rand.Next(size)].Cashier;
                int Sales_date_day = rand.Next(1, 32);
                int Sales_date_mouth = rand.Next(1, 13);
                int Sales_date_year = rand.Next(2001, 2026);
                Date Sales_date = new Date(
                    Sales_date_day,
                    Sales_date_mouth,
                    Sales_date_year
                );

                Sales s = new Sales(Sales_article_id, Sales_article_category, Sales_count, Sales_cashier, Sales_date);
                newSales.Add(s);
            }

            return newSales;
        }

        /// <summary>
        /// Генерирует коллекцию тестовых данных о товаров.
        /// </summary>
        /// <param name="count">
        /// Количество генерируемых записей о товарах.
        /// Должно быть положительным числом (больше 0).
        /// </param>
        /// <param name="goods">
        /// Исходная коллекция товаров, на основе которой генерируются новые данные.
        /// Может использоваться для сохранения уже существующих записей.
        /// </param>
        /// <returns>
        /// Новая коллекция <see cref="Vector{Goods}"/>, содержащая:
        /// <list type="bullet">
        ///     <item><description>Все элементы из исходной коллекции <paramref name="goods"/></description></item>
        ///     <item><description>Добавленные сгенерированные записи (в количестве <paramref name="count"/>)</description></item>
        /// </list>
        /// </returns>
        public static Vector<Goods> GenerateGoods(int count, Vector<Goods> goods)
        {
            int size = goods.Count;
            Random rand = new Random();
            Vector<Goods> newGoods = new Vector<Goods>(count);

            for (int i = 0; i < count; i++)
            {
                // rand generation by int rand and by file
                int Goods_article_id = rand.Next(10000, 99999);
                Article.Category Goods_article_category = (Article.Category)rand.Next(
                    0,
                    Enum.GetValues(typeof(Article.Category)).Length
                );

                string Goods_name = goods[rand.Next(size)].Name;
                double Goods_price = rand.NextDouble() * Math.Pow(10, rand.Next(1, 5));

                Goods g = new Goods(Goods_article_id, Goods_article_category, Goods_name, Goods_price);
                newGoods.Add(g);
            }

            return newGoods;
        }
    }
}
