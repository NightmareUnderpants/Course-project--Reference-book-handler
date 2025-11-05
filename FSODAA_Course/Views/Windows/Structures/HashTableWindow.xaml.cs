using CoreLogic.Classes;
using CoreLogic.FileHandler;
using CoreLogic.FileHandler.String;
using CoreLogic.Structures;
using CoreLogic.Vector;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace FSODAA_Course.Views.Windows.Structures
{
    /// <summary>
    /// Interaction logic for HashTableWindow.xaml
    /// </summary>
    public partial class HashTableWindow : Window
    {
        private int _windowInputWidth = 300;
        private int _windowInputHeight = 150;

        private HashTable _hashTableGoods;
        private Tree<Article> _articleTree;

        private CircularLinkedList<Goods> _goods;

        public HashTableWindow(HashTable hashTable, Tree<Article> articleTree, CircularLinkedList<Goods> allGoods)
        {
            InitializeComponent();
            this.Closing += HashTableWindow_Closing;

            _hashTableGoods = hashTable;
            _articleTree = articleTree;
            _goods = allGoods;
        }

        private void ViewHashTable_Click(object sender, RoutedEventArgs e)
            => UpdateHashTableView();

        private void InitializeHashFromFile_Click(object sender, RoutedEventArgs e)
            => InitializeHashFromFile();

        private void SaveHashToFile_Click(object sender, RoutedEventArgs e)
            => SaveHashToFile();

        private void SearchHashTable_Click(object sender, RoutedEventArgs e)
            => SearchHashTable();

        private void AddHashTable_Click(object sender, RoutedEventArgs e)
            => AddHashTable();

        private void DeleteHashTable_Click(object sender, RoutedEventArgs e)
            => DeleteHashTable();

        private void HashDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
            => HashDataGrid_LoadingRowHandler(e);

        private void HashTableWindow_Closing(object? sender, CancelEventArgs e)
            => HashTableWindow_ClosingHandler(e);

        #region Implementation Methods
        private void InitializeHashFromFile()
        {
            try
            {
                // 1. Читаем товары из файла в единый список
                CircularLinkedList<Goods> goodsList = FileHandler.ReadGoodsFromFile("TestGoods.txt");
                Console.WriteLine($"Initialize HashTable From File\n");

                int count = 0;

                // 2. Проходим по списку и добавляем КАЖДЫЙ узел в структуры данных
                var enumerator = goodsList.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Goods currentGood = enumerator.Current;

                    // 3. Добавляем товар в ХЕШ-ТАБЛИЦУ (передаём ССЫЛКУ НА УЗЕЛ)
                    // ВАЖНО: Нужен метод, который возвращает ссылку на текущий узел итератора
                    if (TryGetNodeFromEnumerator(enumerator, out var nodeRef))
                    {
                        if (_hashTableGoods.Add(currentGood.Article, nodeRef))
                        {
                            count++;
                            // 4. Добавляем КЛЮЧ в дерево статей (без значения)
                            _articleTree.CreateKey(currentGood.Article);
                        }
                    }
                }

                UpdateHashTableView();

                MessageBox.Show($"Успешно загружено {count} записей", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveHashToFile()
        {
            try
            {
                if (_goods == null)
                {
                    MessageBox.Show("Не удалось обратиться к Списку.", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                FileHandler.SaveToFile("SalesGoods.txt", _goods);

                MessageBox.Show($"Успешно сохранено {_goods.Count} записей в файл SalesSave.txt.", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchHashTable()
        {
            Article article = new Article();

            if (!TryParseArticle(out article))
                return;

            if (!Search.SearchByArticle(_hashTableGoods, _articleTree, article, out Goods goods, out Vector<Sales> salesVector))
            {
                MessageBox.Show($"Товар с артикулом {article} не найден.", "Не найдено",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                GoodsPanel.DataContext = null;
                SalesDataGrid.ItemsSource = null;
                return;
            }

            GoodsPanel.DataContext = goods;

            var salesList = new List<Sales>();
            for (int i = 0; i < salesVector.Count; i++)
            {
                var s = salesVector[i];
                if (!s.Equals(default(Sales)))
                    salesList.Add(s);
            }

            SalesDataGrid.ItemsSource = salesList;

            if (_hashTableGoods.Capacity == 0)
                MessageBox.Show("Vector is empty!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void AddHashTable()
        {
            try
            {
                Goods goods = new Goods();

                if (!TryParseGoods(out goods))
                    return;

                var nodeRef = _goods.AddLastAndGetNode(goods);
                if (nodeRef == null)
                {
                    MessageBox.Show("Не удалось добавить товар в список.", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (_hashTableGoods.Add(goods.Article, nodeRef))
                {
                    // 3. Создаём ключ в дереве статей (если его ещё нет)
                    _articleTree.CreateKey(goods.Article);

                    // 4. Обновляем отображение
                    UpdateHashTableView();

                    MessageBox.Show($"Успешно добавлена запись", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Дубликат: удаляем из списка, так как он не добавлен в ХТ
                    _goods.Remove(nodeRef.Value);
                    MessageBox.Show($"Товар с артикулом {goods.Article} уже существует.", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                UpdateHashTableView();

                MessageBox.Show($"Успешно загружена запись", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
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
                Goods goods = new Goods();

                if (!TryParseGoods(out goods))
                    return;

                if (!_hashTableGoods.Find(goods.Article, out var nodeRef))
                {
                    MessageBox.Show($"Товар с артикулом {goods.Article} не найден для удаления.", "Не найдено",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_hashTableGoods.Remove(goods.Article))
                {
                    _goods.Remove(nodeRef.Value);

                    _articleTree.Remove(goods.Article);
                }
                else
                {
                    MessageBox.Show($"Ошибка при удалении из хеш-таблицы.", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var main = Owner as MainWindow;
                if (main == null)
                {
                    MessageBox.Show("Главное окно не найдено.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var treeDate = main.treeDate;

                treeDate.RemoveNodeReferencesByArticle(goods.Article);
                treeDate.RemoveEmptyNodes();

                UpdateHashTableView();

                MessageBox.Show($"Успешно удалена запись", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                       MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateHashTableView()
        {
            try
            {
                // 1. Получаем вектор ячеек из хеш-таблицы
                Vector<HashTable.Hash> hashVector = _hashTableGoods.HashTableToVector();

                // 2. Создаем список для отображения с полными данными
                var displayItems = new List<DisplayHashItem>();

                for (int i = 0; i < hashVector.Count; i++)
                {
                    var hashCell = hashVector[i];

                    // 3. Создаем элемент для отображения
                    var displayItem = new DisplayHashItem
                    {
                        Index = i,
                        Status = GetStatusText(hashCell.Status),
                        Key = hashCell.Key.ToString()
                    };

                    // 4. Заполняем данными о товаре, если ячейка занята
                    if (hashCell.Status == 1 && hashCell.NodeRef != null)
                    {
                        var goods = hashCell.NodeRef.Value;
                        displayItem.Article = goods.Article.ToString();
                        displayItem.Name = goods.Name;
                        displayItem.Price = goods.Price;
                    }

                    displayItems.Add(displayItem);
                }

                // 5. Привязываем к DataGrid
                HashDataGrid.ItemsSource = displayItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления таблицы: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HashDataGrid_LoadingRowHandler(DataGridRowEventArgs e)
        {
            // Присваиваем индекс строки (начиная с 1)
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void HashTableWindow_ClosingHandler(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private bool TryParseArticle(out Article article)
        {
            article = new Article();

            var dlg = new UnputMessage("Введите Артикул (OTH-99999):") { Owner = this };
            dlg.Width = _windowInputWidth;
            dlg.Height = _windowInputHeight;

            if (dlg.ShowDialog() != true)
                return false;

            string raw = dlg.InputText;

            if (!Article.TryParse(raw, out article))
            {
                MessageBox.Show("Не верный формат.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private bool TryParseGoods(out Goods goods)
        {
            goods = new Goods();

            var dlg = new UnputMessage("Введите Товар (OTH-99999;Name;Price):") { Owner = this };
            dlg.Width = _windowInputWidth;
            dlg.Height = _windowInputHeight;

            if (dlg.ShowDialog() != true)
                return false;

            string raw = dlg.InputText;

            goods = StringHandler.StringToGoods(raw);

            return true;
        }

        private bool TryGetNodeFromEnumerator<T>(CircularLinkedList<T>.Enumerator enumerator,
            out CircularLinkedList<T>.Node node)
        {
            node = enumerator.CurrentNode;
            return node != null;
        }
        #endregion

        private class DisplayHashItem
        {
            public int Index { get; set; }
            public string Status { get; set; }
            public string Key { get; set; }
            public string Article { get; set; }
            public string Name { get; set; }
            public double Price { get; set; }
        }

        private string GetStatusText(int status)
        {
            return status switch
            {
                0 => "Пусто",
                1 => "Занято",
                2 => "Удалено",
                _ => "Неизвестно"
            };
        }
    }
}