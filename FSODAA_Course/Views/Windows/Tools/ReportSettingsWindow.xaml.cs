using CoreLogic.Structures;
using System;
using System.Windows;

namespace FSODAA_Course
{
    public partial class ReportSettingsWindow : Window
    {
        public enum DateFilterMode { All, Single, Range }
        public DateFilterMode SelectedDateMode { get; private set; }
        public Date? SingleDate { get; private set; }
        public Date? StartDate { get; private set; }
        public Date? EndDate { get; private set; }
        public bool IsSpecificArticle { get; private set; }
        public Article? SpecificArticle { get; private set; }
        public string? SpecificSeller { get; private set; }

        public ReportSettingsWindow()
        {
            InitializeComponent();
            DateFilterType.SelectionChanged += DateFilterType_SelectionChanged;
            SpecificArticleRadio.Checked += OnSpecificArticleChecked;
            SpecificArticleRadio.Unchecked += OnSpecificArticleUnchecked;
        }

        private void DateFilterType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            switch (DateFilterType.SelectedIndex)
            {
                case 0: // Все даты
                    SingleDatePanel.Visibility = Visibility.Collapsed;
                    DateRangePanel.Visibility = Visibility.Collapsed;
                    break;
                case 1: // Конкретная дата
                    SingleDatePanel.Visibility = Visibility.Visible;
                    DateRangePanel.Visibility = Visibility.Collapsed;
                    break;
                case 2: // Период
                    SingleDatePanel.Visibility = Visibility.Collapsed;
                    DateRangePanel.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void OnSpecificArticleChecked(object sender, RoutedEventArgs e)
        {
            ArticleInput.IsEnabled = true;
        }

        private void OnSpecificArticleUnchecked(object sender, RoutedEventArgs e)
        {
            ArticleInput.IsEnabled = false;
        }

        private bool TryParseDate(string input, out Date date)
        {
            date = new Date();
            if (string.IsNullOrWhiteSpace(input)) return false;
            var parts = input.Split('.');
            if (parts.Length != 3) return false;
            if (!int.TryParse(parts[0], out int d) ||
                !int.TryParse(parts[1], out int m) ||
                !int.TryParse(parts[2], out int y)) return false;
            date = new Date(d, m, y);
            return true;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Обработка даты
            SelectedDateMode = (DateFilterMode)DateFilterType.SelectedIndex;
            if (SelectedDateMode == DateFilterMode.Single)
            {
                if (!TryParseDate(SingleDateInput.Text, out Date sd))
                {
                    MessageBox.Show("Неверный формат даты.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                SingleDate = sd;
            }
            else if (SelectedDateMode == DateFilterMode.Range)
            {
                if (!TryParseDate(StartDateInput.Text, out Date start) ||
                    !TryParseDate(EndDateInput.Text, out Date end))
                {
                    MessageBox.Show("Неверный формат даты.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (start.CompareTo(end) > 0)
                {
                    MessageBox.Show("Начальная дата не может быть позже конечной.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                StartDate = start;
                EndDate = end;
            }

            // Обработка артикула
            IsSpecificArticle = SpecificArticleRadio.IsChecked == true;
            if (IsSpecificArticle)
            {
                if (!Article.TryParse(ArticleInput.Text, out Article art))
                {
                    MessageBox.Show("Неверный формат артикула.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                SpecificArticle = art;
            }

            // Обработка продавца (только это поле добавлено вместо названия)
            SpecificSeller = string.IsNullOrWhiteSpace(SellerInput.Text) ? null : SellerInput.Text.Trim();

            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}