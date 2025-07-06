using CoreLogic.Structures;
using System;

namespace CoreLogic.FileHandler.String
{
    public class StringHandler
    {
        /// <summary>
        /// Преобразует строку в структуру данных о продаже (Sales).
        /// </summary>
        /// <param name="str">
        /// Строка для парсинга в формате: "(артикул);(количество);(имя кассира);(дата)".
        /// <para>Пример: "EL-12345;2;Иванский Иван Иванов;01.01.2020"</para>
        /// </param>
        /// <returns>
        /// Структура <see cref="Sales"/>, содержащая распарсенные данные о продаже.
        /// </returns>
        /// <exception cref="FormatException">
        /// Выбрасывается, если:
        /// <list type="bullet">
        ///     <item><description>строка имеет неверный формат</description></item>
        ///     <item><description>не удалось преобразовать числовые значения</description></item>
        ///     <item><description>дата имеет неверный формат</description></item>
        /// </list>
        /// </exception>
        public static Sales StringToSales(string str)
        {
            string[] parts = str.Split(';');
            try
            {
                if (parts.Length != 4)
                {
                    throw new FormatException("Invalid input format. Expected: (article);(count);(cashier);(date)");
                }

                if (!Article.TryParse(parts[0], out Article article))
                {
                    throw new FormatException("Invalid article number format");
                }

                if (!int.TryParse(parts[1], out int count))
                {
                    throw new FormatException("Invalid count number format");
                }

                string cashier = parts[2].Trim();
                if (string.IsNullOrEmpty(cashier))
                {
                    throw new FormatException("Cashier name cannot be empty");
                }

                string[] dateParts = parts[3].Split('.');
                if (dateParts.Length != 3)
                {
                    throw new FormatException("Invalid date format. Expected: (day).(mouth).(year)");
                }

                if (!int.TryParse(dateParts[0], out int day) ||
                    !int.TryParse(dateParts[1], out int mouth) ||
                    !int.TryParse(dateParts[2], out int year))
                {
                    throw new FormatException("Invalid date components");
                }

                if (day < 1 || day > 31 ||
                    mouth < 1 || mouth > 12 ||
                    year < 1)
                {
                    throw new FormatException("Invalid date values");
                }

                return new Sales(article, count, cashier, new Date(day, mouth, year));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}. Save default value");
                return new Sales(new Article(), 0, "Unknown", new Date(01,01,1900));
            }
        }

        /// <summary>
        /// Преобразует строку в структуру данных о продаже (Goods).
        /// </summary>
        /// <param name="str">
        /// Строка для парсинга в формате: "(артикул);(название);(цена)".
        /// <para>Пример: "EL-12345;Молоко;85,50"</para>
        /// </param>
        /// <returns>
        /// Структура <see cref="Goods"/>, содержащая распарсенные данные о продаже.
        /// </returns>
        /// <exception cref="FormatException">
        /// Выбрасывается, если:
        /// <list type="bullet">
        ///     <item><description>строка имеет неверный формат</description></item>
        ///     <item><description>не удалось преобразовать числовые значения</description></item>
        ///     <item><description>дата имеет неверный формат</description></item>
        /// </list>
        /// </exception>
        public static Goods StringToGoods(string str)
        {
            string[] parts = str.Split(';');
            if (parts.Length != 3)
            {
                throw new FormatException("Invalid input format. Expected: (article);(count);(cashier);(date)");
            }

            if (!Article.TryParse(parts[0], out Article article))
            {
                throw new FormatException("Invalid article number format");
            }

            string name = parts[1].Trim();
            if (string.IsNullOrEmpty(name))
            {
                throw new FormatException("Name cannot be empty");
            }

            if (!double.TryParse(parts[2], out double price))
            {
                throw new FormatException("Invalid price number format");
            }

            return new Goods(article, name, price);
        }
    }
}
