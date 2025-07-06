using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FSODAA_Course
{
    /// <summary>
    /// Interaction logic for DateInputWindow.xaml
    /// </summary>
    public partial class UnputMessage : Window
    {
        public string InputText => TxtInput.Text.Trim();

        public UnputMessage(string title = "Введите значение")
        {
            InitializeComponent();
            Title = title;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
