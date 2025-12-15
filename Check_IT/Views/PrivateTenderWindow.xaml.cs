using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using System.Linq;
using ClosedXML.Excel;
using Check_IT.Services;
using Check_IT.Models;

namespace Check_IT
{
    public partial class PrivateTenderWindow : Window
    {
        public ObservableCollection<ComparisonItem> ComparisonItems { get; } = new();

        private string? _selectedFilePath;

        public PrivateTenderWindow()
        {
            InitializeComponent();

            DataContext = this;

            HeaderTitle.Text = "Завантаження Excel документа";
            HeaderSubtitle.Text = "Перетягніть файл або виберіть Excel документ для перевірки та аналізу.";
            DropHint.Text = "Перетягніть файл сюди або натисніть Select file";
            InfoText.Text = "Підтримуються формати: .xlsx, .xls. Після аналізу буде показано порівняння цін.";

            if (ResultsGrid.Columns.Count >= 3)
            {
                ResultsGrid.Columns[0].Header = "Назва товару";
                ResultsGrid.Columns[1].Header = "Ціна з тендеру, грн";
                ResultsGrid.Columns[2].Header = "Ціна з Rozetka, грн";
            }
        }

        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx;*.xls)|*.xlsx;*.xls",
                Multiselect = false
            };

            if (dlg.ShowDialog(this) == true)
                SetSelectedFile(dlg.FileName);
        }

        private void DropArea_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;

            e.Handled = true;
        }

        private void DropArea_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
                SetSelectedFile(files[0]);
        }

        private void SetSelectedFile(string path)
        {
            _selectedFilePath = path;
            SelectedFileName.Text = System.IO.Path.GetFileName(path);
            ProcessButton.IsEnabled = true;
            ComparisonItems.Clear();
            InfoText.Text = $"Файл обрано: {SelectedFileName.Text}. Натисніть Analyze.";
        }

        private async void ProcessButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFilePath)) return;

            ProcessButton.IsEnabled = false;
            SelectFileButton.IsEnabled = false;
            InfoText.Text = "Обробка файлу...";

            try
            {
                var results = await ProcessExcelWithRozetkaAsync(_selectedFilePath!);

                ComparisonItems.Clear();
                foreach (var it in results)
                    ComparisonItems.Add(it);

                InfoText.Text = $"Аналіз завершено. Знайдено {ComparisonItems.Count} позицій.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                InfoText.Text = "Помилка під час обробки файлу.";
            }
            finally
            {
                ProcessButton.IsEnabled = true;
                SelectFileButton.IsEnabled = true;
            }
        }

        private async Task<ComparisonItem[]> ProcessExcelWithRozetkaAsync(string filePath)
        {
            var products = new List<ComparisonItem>();

            using (var workbook = new XLWorkbook(filePath))
            {
                var ws = workbook.Worksheets.First();
                foreach (var row in ws.RowsUsed().Skip(1))
                {
                    var name = row.Cell(1).GetString();
                    if (string.IsNullOrWhiteSpace(name)) continue;

                    decimal? excelPrice = null;
                    if (decimal.TryParse(row.Cell(2).GetString().Replace(',', '.'), out var parsed))
                        excelPrice = parsed;

                    products.Add(new ComparisonItem { Name = name, Price = excelPrice });
                }
            }

            var scraper = new RozetkaScraper();

            int index = 1;
            foreach (var item in products)
            {
                InfoText.Text = $"🔎 Пошук {index}/{products.Count}: {item.Name}";
                index++;

                try
                {
                    var found = await scraper.FindProductsAsync(item.Name!, 10, true, CancellationToken.None);

                    if (found.Any())
                    {
                        var first = found.FirstOrDefault();
                        if (first != null && decimal.TryParse(first.Price, out var firstPrice))
                            item.RozetkaPrice = firstPrice;
                    }
                }
                catch
                {
                    item.RozetkaPrice = null;
                }
            }

            InfoText.Text = "✅ Аналіз завершено.";
            return products.ToArray();
        }
    }
}
