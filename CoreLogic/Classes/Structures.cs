using System;

namespace CoreLogic.Structures
{
    /// <summary>
    /// Структура, представляющая данные о товаре.
    /// <para><b>Содержит поля:</b></para>
    /// <list type="bullet">
    ///     <item><description><c>article</c> - артикул товара</description></item>
    ///     <item><description><c>name</c> - название товара</description></item>
    ///     <item><description><c>price</c> - цена товара</description></item>
    /// </list>
    /// </summary>
    public struct Goods
    {
        public Article Article { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }

        public Goods(Article article, string name, double price)
        {
            Article = article;
            Name = name;
            Price = price;
        }

        public Goods(int idArticle, Article.Category category, string name, double price)
        {
            Article article = new Article();
            article.id = idArticle;
            article.category = category;

            Article = article;
            Name = name;
            Price = price;
        }

        public override string ToString()
        {
            return $"{Article};{Name};{Price}";
        }
    }

    /// <summary>
    /// Структура, представляющая данные о продаже.
    /// <para><b>Содержит поля:</b></para>
    /// <list type="bullet">
    ///     <item><description><c>article</c> - артикул товара</description></item>
    ///     <item><description><c>count</c> - количество проданных единиц</description></item>
    ///     <item><description><c>cashier</c> - имя кассира</description></item>
    ///     <item><description><c>date</c> - дата продажи</description></item>
    /// </list>
    /// </summary>
    public struct Sales
    {
        public Article Article { get; set; }
        public int Count { get; set; }
        public string Cashier { get; set; }
        public Date Date { get; set; }

        public Sales(Article article, int count, string cashier, Date date)
        {
            Article = article;
            Count = count;
            Cashier = cashier;
            Date = date;
        }

        public Sales(int idArticle, Article.Category category, int count, string cashier, Date date)
        {
            Article article = new Article();
            article.id = idArticle;
            article.category = category;

            Article = article;
            Count = count;
            Cashier = cashier;
            Date = date;
        }

        public override string ToString()
        {
            if (Article.id == 0 || Count == 0) return "Empty";
            return $"{Article};{Count};{Cashier};{Date}";
        }

        public override bool Equals(object obj)
        {
            if (obj is Sales other) return Article.Equals(other.Article)
                && Count == other.Count
                && Cashier == other.Cashier
                && Date.Equals(other.Date);
            return false;
        }
    }

    /// <summary>
    /// Структура, представляющая данные о дате.
    /// <para><b>Содержит поля:</b></para>
    /// <list type="bullet">
    ///     <item><description><c>day</c> - день</description></item>
    ///     <item><description><c>mouth</c> - месяц</description></item>
    ///     <item><description><c>year</c> - год</description></item>
    /// </list>
    /// </summary>
    public struct Date: IComparable<Date>
    {
        public int day;
        public int month;
        public int year;

        public Date(int day, int month, int year)
        {
            this.day = day;
            this.month = month;
            this.year = year;
        }

        public static bool TryParse(string inputText, out Date date)
        {
            date = default;

            if (string.IsNullOrWhiteSpace(inputText))
                return false;

            var parts = inputText.Split('.');
            if (parts.Length != 3)
                return false;

            if (!int.TryParse(parts[0], out int day) ||
                !int.TryParse(parts[1], out int month) ||
                !int.TryParse(parts[2], out int year))
                return false;

            // Простейшая валидация диапазонов
            if (year < 1 || month < 1 || month > 12 || day < 1 || day > DateTime.DaysInMonth(year, month))
                return false;

            date = new Date(day, month, year);
            return true;
        }

        public int CompareTo(Date other)
        {
            if (year != other.year)
                return year.CompareTo(other.year);
            if (month != other.month)
                return month.CompareTo(other.month);
            return day.CompareTo(other.day);
        }


        public override string ToString()
        {
            return (day > 9 ? $"{day}." : $"0{day}.")
                + (month > 9 ? $"{month}." : $"0{month}.")
                + year;
        }        
    }

    /// <summary>
    /// Подструктура артикула, содержащая точные данные о артикуле товара
    /// <para><b>Содержит поля:</b></para>
    /// <list type="bullet">
    ///     <item><description><c>category</c> - категория товара. Тип данных enum Category</description></item>
    ///     <item><description><c>id</c> - id товара</description></item>
    /// </list>
    /// </summary>
    public struct Article : IComparable<Article>
    {
        public enum Category
        {
            EL, // электроника
            CL, // одежда
            FUR, // мебель
            OTH // Другое
        }
        public Category category;

        public int id;

        public static bool TryParse(string str, out Article result)
        {
            result = default;
            string[] parts = str.Split('-');

            if (parts.Length != 2)
                return false;

            if (!Enum.TryParse(parts[0], out Article.Category category))
                return false;

            if (!int.TryParse(parts[1], out int id))
                return false;

            result = new Article()
            {
                category = category,
                id = id
            };
            return true;
        }

        public override string ToString()
        {
            string str = string.Empty;
            switch (category)
            {
                case Category.EL:
                    str += "EL";
                    break;
                case Category.CL:
                    str += "CL";
                    break;
                case Category.FUR:
                    str += "FUR";
                    break;
                case Category.OTH:
                    str += "OTH";
                    break;
                default:
                    break;
            }
            return $"{str}-{id}";
        }

        public int CompareTo(Article other)
        {
            int categoryCompare = category.CompareTo(other.category);
            if (categoryCompare != 0)
                return categoryCompare;

            return id.CompareTo(other.id);
        }
    }
}
