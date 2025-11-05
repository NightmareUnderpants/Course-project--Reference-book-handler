using CoreLogic.Classes;
using CoreLogic.FileHandler.String;
using CoreLogic.Structures;
using CoreLogic.Vector;
using System;
using System.Collections.Generic;
using System.IO;

namespace CoreLogic.FileHandler
{
    public class FileHandler
    {
        private static string GetDesktopPath() =>
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        private static CircularLinkedList<T> ReadFromFile<T>(string fileName, Func<string, T> converter)
        {
            string path = Path.Combine(GetDesktopPath(), fileName);

            if (!File.Exists(path))
                throw new FileNotFoundException($"Файл не найден: {fileName}", path);

            string[] lines = File.ReadAllLines(path);
            var list = new CircularLinkedList<T>();

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (!string.IsNullOrEmpty(line))
                {
                    try
                    {
                        T item = converter(line);
                        list.AddLast(item); // ← Используйте правильное имя метода из вашего класса
                    }
                    catch (Exception ex)
                    {
                        // Опционально: логирование ошибок парсинга конкретной строки
                        Console.WriteLine($"Ошибка парсинга строки {i + 1} в файле {fileName}: {ex.Message}");
                        // Можно либо пропустить строку, либо прервать загрузку
                    }
                }
            }

            return list;
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
        public static CircularLinkedList<Sales> ReadSalesFromFile(string fileName) =>
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
        public static CircularLinkedList<Goods> ReadGoodsFromFile(string fileName) =>
            ReadFromFile(fileName, StringHandler.StringToGoods);

        public static void SaveToFile<T>(string fileName, CircularLinkedList<T> items)
        {
            Console.WriteLine($"SaveToFile: fileName={fileName}, items.Count={items.Count}\n");

            string path = Path.Combine(GetDesktopPath(), fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            // Используем список для сбора строк, так как мы не знаем точное количество заранее
            // (если в списке есть пустые узлы или логика фильтрации)
            var lines = new List<string>();

            // Используем итератор для прохода по списку ОДИН раз
            var enumerator = items.GetEnumerator();
            while (enumerator.MoveNext())
            {
                string line = enumerator.Current?.ToString() ?? string.Empty;
                lines.Add(line);
                Console.WriteLine(line);
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
        public static void SaveSalesToFile(string fileName, CircularLinkedList<Sales> sales) =>
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
        public static void SaveGoodsToFile(string fileName, CircularLinkedList<Goods> goods) =>
            SaveToFile(fileName, goods);
    }
}
