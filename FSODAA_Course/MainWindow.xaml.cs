using CoreLogic.Classes;
using CoreLogic.Structures;
using FSODAA_Course.Views.Windows.Structures;
using System.Windows;

namespace FSODAA_Course
{
    public partial class MainWindow : Window
    {
        // Tree with Date
        public Tree<Date> treeDate = new Tree<Date>();

        // HashTable&Tree with Article
        public HashTable hashTableSales = new HashTable(10);
        public Tree<Article> articleTree = new Tree<Article>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenHashTableWindow_Click(object sender, RoutedEventArgs e)
        {
            var win = new HashTableWindow(hashTableSales, articleTree)
            {
                Owner = this
            };
            win.Show();
        }

        private void OpenTreeWindow_Click(object sender, RoutedEventArgs e)
        {
            var win = new TreeWindow(treeDate)
            {
                Owner = this
            };
            win.Show();
        }
    }
}