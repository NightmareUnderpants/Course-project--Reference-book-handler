using CoreLogic.Structures;
using CoreLogic.Vector;
using CoreLogic.FileHandler.String;
using System.IO;
using System;

namespace CoreLogic.FileHandler
{
    public class FileHandler
    {
        private static string GetDesktopPath() =>
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        private static Vector<T> ReadFromFile<T>(string fileName, Func<string, T> converter)
        {
            string path = Path.Combine(GetDesktopPath(), fileName);

            if (!File.Exists(path))
                throw new FileNotFoundException("File not found", path);

            string[] lines = File.ReadAllLines(path);

            Vector<T> result = new Vector<T>();

            for (int i = 0; i < lines.Length; i++)
            {
                result.Add(converter(lines[i]));
            }

            return result;
        }


        /// <summary>
        /// Считывает данные о продажах из текстового файла и возвращает их в виде коллекции <see cref="Sales"/>.
        /// </summary>
        /// <param name="fileName">
        /// Имя файла с расширением (например: "goods_list.txt").
        /// </param>
        /// <returns>
        /// Коллекция объектов <see cref="Sales"/>. Если файл пуст, возвращается пустая коллекция.
        /// </returns>
        /// <remarks>
        /// Каждая строка файла должна содержать данные одной продажи в установленном формате.
        /// </remarks>
        public static Vector<Sales> ReadSalesFromFile(string fileName) =>
            ReadFromFile(fileName, StringHandler.StringToSales);

        /// <summary>
        /// Считывает данные о товарах из текстового файла и возвращает их в виде коллекции <see cref="Goods"/>.
        /// </summary>
        /// <param name="fileName">
        /// Имя файла с расширением (например: "goods_list.txt").
        /// </param>
        /// <returns>
        /// Коллекция объектов <see cref="Goods"/>. Если файл пуст, возвращается пустая коллекция.
        /// </returns>
        /// <remarks>
        /// Каждая строка файла должна содержать данные одного товара в установленном формате.
        /// </remarks>
        public static Vector<Goods> ReadGoodsFromFile(string fileName) =>
            ReadFromFile(fileName, StringHandler.StringToGoods);

        private static void SaveToFile<T>(string fileName, Vector<T> items)
        {
            string path = Path.Combine(GetDesktopPath(), fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            string[] lines = new string[items.Count];

            for (int i = 0; i < items.Count; i++)
            {
                lines[i] = items[i].ToString();
            }

            File.WriteAllLines(path, lines);
        }


        /// <summary>
        /// Сохраняет коллекцию продаж <see cref="Sales"/> в текстовый файл на рабочем столе.
        /// </summary>
        /// <param name="fileName">
        /// Имя файла с расширением (например: "goods_list.txt").
        /// <para>Поддерживаются только текстовые форматы (.txt, .csv и т.п.).</para>
        /// </param>
        /// <param name="sales">
        /// Коллекция товаров для сохранения (<see cref="Vector{T}"/> где T - <see cref="Sales"/>).
        /// Если коллекция пуста, будет создан пустой файл.
        /// </param>
        /// <remarks>
        /// <para>Файл сохраняется в папке рабочего стола текущего пользователя.</para>
        /// <para>Если файл уже существует, он будет перезаписан без предупреждения.</para>
        /// </remarks>
        public static void SaveSalesToFile(string fileName, Vector<Sales> sales) =>
            SaveToFile(fileName, sales);


        /// <summary>
        /// Сохраняет коллекцию товаров <see cref="Goods"/> в текстовый файл на рабочем столе.
        /// </summary>
        /// <param name="fileName">
        /// Имя файла с расширением (например: "goods_list.txt").
        /// <para>Поддерживаются только текстовые форматы (.txt, .csv и т.п.).</para>
        /// </param>
        /// <param name="goods">
        /// Коллекция товаров для сохранения (<see cref="Vector{T}"/> где T - <see cref="Goods"/>).
        /// Если коллекция пуста, будет создан пустой файл.
        /// </param>
        /// <remarks>
        /// <para>Файл сохраняется в папке рабочего стола текущего пользователя.</para>
        /// <para>Если файл уже существует, он будет перезаписан без предупреждения.</para>
        /// </remarks>
        public static void SaveGoodsToFile(string fileName, Vector<Goods> goods) =>
            SaveToFile(fileName, goods);
    }
}
