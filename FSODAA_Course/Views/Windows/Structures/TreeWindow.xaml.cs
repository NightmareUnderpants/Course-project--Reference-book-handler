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

        private int _windowInputWidth = 300;
        private int _windowInputHeight = 150;
        public TreeWindow(Tree<Date> treeDate)
        {
            InitializeComponent();
            this.Closing += TreeWindow_Closing;
            _treeDate = treeDate;

            // Отображение сразу после инициализации
            TreeDisplay.Text = _treeDate.Display();
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

        private void GenerateReport_Click(object sender, RoutedEventArgs e)
            => GenerateReport(sender, e);

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
                    // Возвращаем предупреждение, если элемент не найден
                    MessageBox.Show($"По дате {date} не найдено ни одной продажи.", "Не найдено", MessageBoxButton.OK, MessageBoxImage.Warning);
                    // Очищаем отображение результатов поиска, если оно есть
                    SalesList.ItemsSource = null;
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
                TreeDisplay.Text = _treeDate.Display(); // Обновляем отображение после добавления
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

                // Проверяем, существует ли узел с такой датой и продажа в нём перед удалением
                if (!_treeDate.Contains(sales.Date))
                {
                    // Возвращаем предупреждение, если элемент (дата) не найден
                    MessageBox.Show($"Узел с датой {sales.Date} не найден в дереве.", "Не найдено", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверим, есть ли конкретная продажа в векторе
                var salesOnDate = _treeDate[sales.Date];
                bool saleFound = false;
                for (int i = 0; i < salesOnDate.Count; i++)
                {
                    if (salesOnDate[i].Equals(sales))
                    {
                        saleFound = true;
                        break;
                    }
                }

                if (!saleFound)
                {
                    // Возвращаем предупреждение, если конкретная продажа не найдена
                    MessageBox.Show($"Продажа {sales} не найдена в узле с датой {sales.Date}.", "Не найдено", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _treeDate.RemoveExactSalesAtDate(sales.Date, sales);
                if (_treeDate[sales.Date].Count == 0)
                    _treeDate.Remove(sales.Date);

                TreeDisplay.Text = _treeDate.Display(); // Обновляем отображение после удаления
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
                TreeDisplay.Text = _treeDate.Display(); // Отображение после инициализации
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

        private void GenerateReport(object sender, RoutedEventArgs e)
        {
            var reportSettings = new ReportSettingsWindow { Owner = this };
            if (reportSettings.ShowDialog() != true)
                return;

            try
            {
                // Собираем все продажи из дерева
                var allSales = new List<Sales>();
                var buffer = new Sales[_treeDate.Count];
                _treeDate.CopyTo(buffer, 0);
                foreach (var sale in buffer)
                {
                    if (!sale.Equals(default(Sales)))
                        allSales.Add(sale);
                }

                // Фильтрация по дате
                var filteredByDate = new List<Sales>();
                foreach (var sale in allSales)
                {
                    bool dateMatch = false;
                    switch (reportSettings.SelectedDateMode)
                    {
                        case ReportSettingsWindow.DateFilterMode.All:
                            dateMatch = true;
                            break;
                        case ReportSettingsWindow.DateFilterMode.Single:
                            if (reportSettings.SingleDate.HasValue)
                                dateMatch = sale.Date.Equals(reportSettings.SingleDate.Value);
                            break;
                        case ReportSettingsWindow.DateFilterMode.Range:
                            if (reportSettings.StartDate.HasValue && reportSettings.EndDate.HasValue)
                            {
                                int cmp1 = sale.Date.CompareTo(reportSettings.StartDate.Value);
                                int cmp2 = sale.Date.CompareTo(reportSettings.EndDate.Value);
                                dateMatch = cmp1 >= 0 && cmp2 <= 0;
                            }
                            break;
                    }
                    if (dateMatch)
                        filteredByDate.Add(sale);
                }

                // Фильтрация по артикулу
                var filteredByArticle = new List<Sales>();
                foreach (var sale in filteredByDate)
                {
                    if (reportSettings.IsSpecificArticle)
                    {
                        if (reportSettings.SpecificArticle.HasValue &&
                            sale.Article.Equals(reportSettings.SpecificArticle.Value))
                        {
                            filteredByArticle.Add(sale);
                        }
                    }
                    else
                    {
                        filteredByArticle.Add(sale);
                    }
                }

                // Фильтрация по продавцу (новый этап)
                var finalSales = new List<Sales>();
                foreach (var sale in filteredByArticle)
                {
                    // Если указан конкретный продавец, фильтруем по нему
                    if (!string.IsNullOrWhiteSpace(reportSettings.SpecificSeller))
                    {
                        // Сравниваем без учета регистра и лишних пробелов
                        if (string.Equals(sale.Cashier?.Trim(),
                                         reportSettings.SpecificSeller.Trim(),
                                         StringComparison.OrdinalIgnoreCase))
                        {
                            finalSales.Add(sale);
                        }
                    }
                    else
                    {
                        // Если продавец не указан, берем все записи
                        finalSales.Add(sale);
                    }
                }

                if (finalSales.Count == 0)
                {
                    MessageBox.Show("По заданным критериям не найдено ни одной записи.", "Информация",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Сортировка по продавцу (если указан конкретный продавец, то записи будут только его)
                finalSales.Sort((x, y) => string.Compare(x.Cashier ?? "", y.Cashier ?? "", StringComparison.Ordinal));

                // Формируем отчёт в строку
                var reportLines = new System.Text.StringBuilder();
                reportLines.AppendLine("Отчёт по продажам");
                reportLines.AppendLine("==================");
                reportLines.AppendLine($"Сформировано: {DateTime.Now:dd.MM.yyyy HH:mm}");
                reportLines.AppendLine();

                if (reportSettings.SelectedDateMode == ReportSettingsWindow.DateFilterMode.All)
                    reportLines.AppendLine("Период: Все даты");
                else if (reportSettings.SelectedDateMode == ReportSettingsWindow.DateFilterMode.Single)
                    reportLines.AppendLine($"Дата: {reportSettings.SingleDate.Value}");
                else if (reportSettings.SelectedDateMode == ReportSettingsWindow.DateFilterMode.Range)
                    reportLines.AppendLine($"Период: с {reportSettings.StartDate.Value} по {reportSettings.EndDate.Value}");

                if (reportSettings.IsSpecificArticle)
                    reportLines.AppendLine($"Артикул: {reportSettings.SpecificArticle.Value}");
                else
                    reportLines.AppendLine("Артикул: Все");

                if (!string.IsNullOrWhiteSpace(reportSettings.SpecificSeller))
                    reportLines.AppendLine($"Продавец: {reportSettings.SpecificSeller}");
                else
                    reportLines.AppendLine("Продавец: Все");

                reportLines.AppendLine();
                reportLines.AppendLine("Артикул\t\tКоличество\tПродавец\t\tДата");
                reportLines.AppendLine("------------------------------------------------------------");

                foreach (var sale in finalSales)
                {
                    reportLines.AppendLine($"{sale.Article}\t{sale.Count}\t\t{sale.Cashier}\t\t{sale.Date}");
                }

                // Сохраняем в файл
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                    FileName = $"Report_Sales_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    System.IO.File.WriteAllText(saveDialog.FileName, reportLines.ToString(), System.Text.Encoding.UTF8);
                    MessageBox.Show($"Отчёт успешно сохранён в файл:\n{saveDialog.FileName}", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка формирования отчёта: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool TryParseDate(out Date date)
        {
            date = new Date();

            var dlg = new UnputMessage("Введите дату (dd.MM.yyyy):") { Owner = this };
            dlg.Width = _windowInputWidth;
            dlg.Height = _windowInputHeight;

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
            dlg.Width = _windowInputWidth;
            dlg.Height = _windowInputHeight;

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