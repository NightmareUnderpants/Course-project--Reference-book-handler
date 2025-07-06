using CoreLogic.Classes;
using CoreLogic.FileHandler;
using CoreLogic.FileHandler.String;
using CoreLogic.Structures;
using CoreLogic.Vector;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Controls;

namespace FSODAA_Course.Views.Windows.Structures
{
    /// <summary>
    /// Interaction logic for HashTableWindow.xaml
    /// </summary>
    public partial class HashTableWindow : Window
    {
        private HashTable _hashTableSales = new HashTable(10);
        private Tree<Article> _articleTree = new Tree<Article>();

        public HashTableWindow(HashTable hashTable, Tree<Article> articleTree)
        {
            InitializeComponent();
            this.Closing += HashTableWindow_Closing;

            _hashTableSales = hashTable;
            _articleTree = articleTree;
        }

        private void ViewHashTable_Click(object sender, RoutedEventArgs e)
        {
            // Обновляем отображение
            UpdateHashTableView();
        }

        private void InitializeHashFromFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Vector<Goods> goods = FileHandler.ReadGoodsFromFile("TestGoods.txt");
                int count = 0;
                for (int i = 0; i < goods.Count; i++)
                {
                    if (_hashTableSales.Add(goods[i]))
                    {
                        count++;
                        _articleTree.CreateKey(goods[i].Article);
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

        private void SearchHashTable_Click(object sender, RoutedEventArgs e)
        {
            Article article = new Article();

            if (!TryParseArticle(out article))
                return;

            if (!Search.SearchByArticle(_hashTableSales, _articleTree, article, out Goods goods, out Vector<Sales> salesVector))
                return;

            GoodsPanel.DataContext = goods;

            var salesList = new List<Sales>();
            for (int i = 0; i < salesVector.Count; i++)
            {
                var s = salesVector[i];
                if (!s.Equals(default(Sales)))
                    salesList.Add(s);
            }

            SalesDataGrid.ItemsSource = salesList;

            if (_hashTableSales.Capacity == 0)
                MessageBox.Show("Vector is empty!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void AddHashTable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Goods goods = new Goods();

                if (!TryParseGoods(out goods))
                    return;

                if (_hashTableSales.Add(goods))
                {
                    _articleTree.CreateKey(goods.Article);
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

        private void DeleteHashTable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Goods goods = new Goods();

                if (!TryParseGoods(out goods))
                    return;

                if (_hashTableSales.Remove(goods))
                {
                    _articleTree.Remove(goods.Article);
                }
                else
                {
                    MessageBox.Show($"Ошибка удаления: не удалось удалить из ХТ", "Ошибка",
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

                treeDate.RemoveSalesByArticleAcrossAllDates(goods.Article);

                treeDate.RemoveEmptyNodes();

                UpdateHashTableView();

                MessageBox.Show($"Успешно удалена запись", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateHashTableView()
        {
            Vector<HashTable.Hash> vector = _hashTableSales.HashTableToVector();

            var items = new List<HashTable.Hash>();

            for (int i = 0; i < vector.Count; i++)
            {
                items.Add(vector[i]);
            }

            HashDataGrid.ItemsSource = items;
        }

        private void HashDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            // Присваиваем индекс строки (начиная с 1)
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private bool TryParseArticle(out Article article)
        {
            article = new Article();

            var dlg = new UnputMessage("Введите Артикул (OTH-99999):") { Owner = this };
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
            if (dlg.ShowDialog() != true)
                return false;

            string raw = dlg.InputText;

            goods = StringHandler.StringToGoods(raw);

            return true;
        }

        private void HashTableWindow_Closing(object? sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
