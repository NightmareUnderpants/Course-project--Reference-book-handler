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
        private int _windowInputWidth = 300;
        private int _windowInputHeight = 150;

        private Tree<Date> _treeDate;
        private CircularLinkedList<Sales> _sales;

        public TreeWindow(Tree<Date> treeDate, CircularLinkedList<Sales> allSales)
        {
            InitializeComponent();
            this.Closing += TreeWindow_Closing;

            _treeDate = treeDate;
            _sales = allSales;
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

                var salesNode = _sales.AddLastAndGetNode(sales);

                _treeDate.Add(sales.Date, salesNode);
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

                var salesReferences = _treeDate.GetNodeReferences(sales.Date);
                CircularLinkedList<Sales>.Node targetNodeRef = null;

                for (int i = 0; i < salesReferences.Count; i++)
                {
                    if (salesReferences[i]?.Value.Equals(sales) == true)
                    {
                        targetNodeRef = salesReferences[i];
                        break;
                    }
                }

                if (targetNodeRef == null)
                {
                    MessageBox.Show($"Продажа {sales} не найдена в узле с датой {sales.Date}.", "Не найдено",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 3. Удаляем ССЫЛКУ из дерева дат
                if (_treeDate.RemoveExactNodeReferenceAtDate(sales.Date, targetNodeRef))
                {
                    // 4. Удаляем САМУ ПРОДАЖУ из общего списка
                    _sales.Remove(targetNodeRef.Value);

                    // 5. Если узел даты стал пустым, удаляем его из дерева
                    if (_treeDate.GetNodeReferences(sales.Date).Count == 0)
                    {
                        _treeDate.Remove(sales.Date);
                    }

                    TreeDisplay.Text = _treeDate.Display();
                    MessageBox.Show($"Успешно удалена продажа", "Успех",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Ошибка при удалении продажи из дерева.", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
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

                CircularLinkedList<Sales> salesFromFile = FileHandler.ReadSalesFromFile("TestSales.txt");
                int count = 0;

                var enumerator = salesFromFile.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Sales currentSale = enumerator.Current;

                    if (!main.articleTree.Contains(currentSale.Article))
                    {
                        Console.WriteLine($"WarningLog: {currentSale.Article} not exists in Goods HashTable!");
                        continue;
                    }

                    bool saleExists = false;
                    var existingReferences = _treeDate.GetNodeReferences(currentSale.Date);

                    for (int j = 0; j < existingReferences.Count; j++)
                    {
                        if (existingReferences[j]?.Value.Equals(currentSale) == true)
                        {
                            saleExists = true;
                            break;
                        }
                    }

                    if (saleExists)
                    {
                        Console.WriteLine($"WarningLog: Sale {currentSale} already exists in tree!");
                        continue;
                    }

                    var saleNodeRef = _sales.AddLastAndGetNode(currentSale);

                    _treeDate.Add(currentSale.Date, saleNodeRef);

                    main.articleTree.Add(currentSale.Article, saleNodeRef);

                    count++;
                }

                TreeDisplay.Text = _treeDate.Display();
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
                FileHandler.SaveSalesToFile("SalesSave.txt", _sales);

                MessageBox.Show($"Успешно сохранено {_sales.Count} записей в файл SalesSave.txt.", "Успех",
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
                var main = Owner as MainWindow;
                if (main == null)
                {
                    MessageBox.Show("Главное окно не найдено.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 1. Получаем все продажи из общего списка
                var allSales = new List<Sales>();
                var enumerator = _sales.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (!enumerator.Current.Equals(default(Sales)))
                        allSales.Add(enumerator.Current);
                }

                if (allSales.Count == 0)
                {
                    MessageBox.Show("В списке нет продаж для формирования отчёта.", "Информация",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 2. Фильтрация по дате
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

                // 3. Фильтрация по артикулу
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

                // 4. Фильтрация по продавцу
                var finalSales = new List<Sales>();
                foreach (var sale in filteredByArticle)
                {
                    if (!string.IsNullOrWhiteSpace(reportSettings.SpecificSeller))
                    {
                        if (!string.IsNullOrWhiteSpace(sale.Cashier) &&
                            string.Equals(sale.Cashier.Trim(),
                                         reportSettings.SpecificSeller.Trim(),
                                         StringComparison.OrdinalIgnoreCase))
                        {
                            finalSales.Add(sale);
                        }
                    }
                    else
                    {
                        finalSales.Add(sale);
                    }
                }

                if (finalSales.Count == 0)
                {
                    MessageBox.Show("По заданным критериям не найдено ни одной записи.", "Информация",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 5. Сортировка по продавцу
                finalSales.Sort((x, y) => string.Compare(x.Cashier ?? "", y.Cashier ?? "", StringComparison.Ordinal));

                // 6. Формируем отчёт с данными из Goods
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
                reportLines.AppendLine("Артикул\tНазвание\t\tКол-во\tЦена\tПродавец\tДата");
                reportLines.AppendLine("--------------------------------------------------------------------------------");

                foreach (var sale in finalSales)
                {
                    string name = "Не найден";
                    double price = 0;

                    if (main.hashTableGoods.Find(sale.Article, out var goods))
                    {
                        name = goods.Value.Name ?? "Без названия";
                        price = goods.Value.Price;
                    }

                    string formattedPrice = price % 1 == 0 ? ((int)price).ToString() : price.ToString("0.##");

                    reportLines.AppendLine($"{sale.Article}\t{name}\t{sale.Count}\t{formattedPrice}\t{sale.Cashier}\t{sale.Date}");
                }

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