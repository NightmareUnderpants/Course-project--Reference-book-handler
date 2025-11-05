using CoreLogic.Classes;
using CoreLogic.Structures;
using FSODAA_Course.Views.Windows.Structures;
using System.Windows;

namespace FSODAA_Course
{
    public partial class MainWindow : Window
    {
        // Единые списки-хранилища для всех данных
        private readonly CircularLinkedList<Goods> _goods = new CircularLinkedList<Goods>();
        private readonly CircularLinkedList<Sales> _sales = new CircularLinkedList<Sales>();

        // Структуры данных для индексации (работают со ссылками!)
        public HashTable hashTableGoods;
        public Tree<Article> articleTree;
        public Tree<Date> treeDate;

        public MainWindow()
        {
            InitializeComponent();

            // Инициализация структур с передачей ссылок на списки
            hashTableGoods = new HashTable(10);
            articleTree = new Tree<Article>();
            treeDate = new Tree<Date>();
        }

        private void OpenHashTableWindow_Click(object sender, RoutedEventArgs e)
        {
            var win = new HashTableWindow(hashTableGoods, articleTree, _goods)
            {
                Owner = this
            };
            win.Show();
        }

        private void OpenTreeWindow_Click(object sender, RoutedEventArgs e)
        {
            var win = new TreeWindow(treeDate, _sales)
            {
                Owner = this
            };
            win.Show();
        }
    }
}