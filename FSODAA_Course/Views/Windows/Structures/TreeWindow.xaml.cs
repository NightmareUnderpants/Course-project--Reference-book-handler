using CoreLogic.Classes;
using CoreLogic.FileHandler;
using CoreLogic.FileHandler.String;
using CoreLogic.Structures;
using CoreLogic.Vector;
using System.ComponentModel;
using System.Windows;

namespace FSODAA_Course.Views.Windows.Structures
{
    public partial class TreeWindow : Window
    {
        private Tree<Date> _treeDate = new Tree<Date>();
        public TreeWindow(Tree<Date> treeDate)
        {
            InitializeComponent();
            this.Closing += TreeWindow_Closing;
            _treeDate = treeDate;
        }

        private void ViewFullTree_Click(object sender, EventArgs e)
            => TreeDisplay.Text = _treeDate.Display();

        private void SearchTree_Click(object sender, RoutedEventArgs e)
            => SearchTree();

        private void DeleteHashTable_Click(object sender, RoutedEventArgs e)
            => DeleteHashTable();

        private void AddTreeNode_Click(object sender, RoutedEventArgs e)
            => AddTreeNode();

        private void InitializeTreeNodeFromFile_Click(object sender, RoutedEventArgs e)
            => InitializeTreeFromFile();

        private void SaveTreeNodeToFile_Click(object sender, RoutedEventArgs e)
            => SaveTreeNodeToFile();

        private void SearchTree()
        {
            try
            {
                if (!TryParseDate(out Date date))
                {
                    MessageBox.Show("Некорректный ввод даты.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Search.SearchByDate(_treeDate, date, out Vector<Sales> salesVector);

                if (salesVector == null || salesVector.Count == 0)
                {
                    MessageBox.Show($"По дате {date} не найдено ни одной продажи.", "Результат поиска", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var salesList = new List<Sales>();
                for (int i = 0; i < salesVector.Count; i++)
                {
                    salesList.Add(salesVector[i]);
                }

                SalesList.ItemsSource = salesList;
                MessageBox.Show($"Найдено {salesList.Count} продаж(и) по дате {date}.", "Результат поиска", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddTreeNode()
        {
            try
            {
                if (!TryParseSales(out Sales sales))
                    return;

                var main = Owner as MainWindow;
                if (main == null)
                {
                    MessageBox.Show("Главное окно не найдено.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var articleTree = main.articleTree;

                if (articleTree[sales.Article] == null)
                {
                    MessageBox.Show("По заданному артиклу не удалось найти запись.", "Предупреждение!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var salesOnDate = _treeDate[sales.Date];

                for (int i = 0; i < salesOnDate.Count; i++)
                {
                    if (salesOnDate[i].Equals(sales))
                    {
                        MessageBox.Show("Такая продажа уже добавлена для этой даты.", "Предупреждение!", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                _treeDate.Add(sales.Date, sales);
                TreeDisplay.Text = _treeDate.Display();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteHashTable()
        {
            try
            {
                if (!TryParseSales(out Sales sales))
                    return;

                var main = Owner as MainWindow;
                if (main == null)
                {
                    MessageBox.Show("Главное окно не найдено.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var articleTree = main.articleTree;

                if (articleTree[sales.Article] == null)
                {
                    MessageBox.Show("По заданному артиклу не удалось найти запись.", "Предупреждение!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _treeDate.RemoveExactSalesAtDate(sales.Date, sales);
                if (_treeDate[sales.Date].Count == 0)
                    _treeDate.Remove(sales.Date);

                TreeDisplay.Text = _treeDate.Display();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeTreeFromFile()
        {
            try
            {
                var main = Owner as MainWindow;
                if (main == null)
                {
                    MessageBox.Show("Главное окно не найдено.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var articleTree = main.articleTree;

                Vector<Sales> sales = FileHandler.ReadSalesFromFile("TestSales.txt");
                int count = 0;
                for (int i = 0; i < sales.Count; i++)
                {
                    if (articleTree.Contains(sales[i].Article))
                    {
                        bool exists = false;

                        if (_treeDate.Contains(sales[i].Date))
                        {
                            var salesOnDate = _treeDate[sales[i].Date];

                            for (int j = 0; j < salesOnDate.Count; j++)
                            {
                                if (salesOnDate[j].Equals(sales[i]))
                                {
                                    exists = true;
                                    break;
                                }
                            }
                        }

                        if (!exists)
                        {
                            _treeDate.Add(sales[i].Date, sales[i]);
                            articleTree.Add(sales[i].Article, sales[i]);
                            count++;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"WarningLog: {sales[i].Article.ToString()} not exists in Search Tree!");
                    }

                }
                MessageBox.Show($"Успешно загружено {count} записей", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveTreeNodeToFile()
        {
            try
            {
                int total = _treeDate.Count;
                if (total == 0)
                {
                    MessageBox.Show("В дереве нет записей для сохранения.", "Информация",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var buffer = new Sales[total];
                _treeDate.CopyTo(buffer, 0);

                var allSales = new Vector<Sales>();
                for (int i = 0; i < buffer.Length; i++)
                    allSales.Add(buffer[i]);

                FileHandler.SaveSalesToFile("SalesSave.txt", allSales);

                MessageBox.Show($"Успешно сохранено {allSales.Count} записей в файл SalesSave.txt.", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool TryParseDate(out Date date)
        {
            date = new Date();

            var dlg = new UnputMessage("Введите дату (dd.MM.yyyy):") { Owner = this };
            if (dlg.ShowDialog() != true)
                return false;

            string raw = dlg.InputText;

            string[] dateParts = raw.Split('.');
            if (dateParts.Length != 3)
            {
                MessageBox.Show("Неверный формат даты.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(dateParts[0], out int day) ||
                !int.TryParse(dateParts[1], out int mouth) ||
                !int.TryParse(dateParts[2], out int year))
            {
                MessageBox.Show("Неверный формат даты.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            date = new Date(day, mouth, year);
            return true;
        }

        private bool TryParseSales(out Sales sales)
        {
            sales = new Sales();

            var dlg = new UnputMessage("Введите Продажи(Артикул, Количество, Продавец, Дата продажи):") { Owner = this };
            if (dlg.ShowDialog() != true)
                return false;

            string raw = dlg.InputText;

            sales = StringHandler.StringToSales(raw);
            return true;
        }

        private void TreeWindow_Closing(object? sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
