using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Check_IT.Interfaces;
using Check_IT.Models;
using Check_IT.Services;

namespace Check_IT
{
    public partial class ProzorroWindow : Window
    {
        public ObservableCollection<ComparisonItem> ProzorroItems { get; } = new();

        private readonly ProzorroProcessor _processor;

        public ProzorroWindow() : this(false, null)
        {
        }

        // test-friendly ctor
        public ProzorroWindow(bool skipInitialize, IAppServices? appServices = null)
        {
            _processor = new ProzorroProcessor(appServices);
            if (!skipInitialize)
            {
                InitializeComponent();

                DataContext = this;

                HeaderTitle.Text = "Пошук тендеру Prozorro";
                HeaderSubtitle.Text = "Введіть ID тендеру або контракту Prozorro.";
                AnalyzeButtonText.Text = "Аналізувати";
                InfoText.Text = "Після аналізу буде показано порівняння цін позицій тендеру з цінами на Rozetka.";
            }
        }

        private async void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            var tenderId = TenderIdTextBox.Text?.Trim();

            if (string.IsNullOrEmpty(tenderId))
            {
                MessageBox.Show(this, "Введіть ID тендеру / контракту", "Помилка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AnalyzeButton.IsEnabled = false;
            ProzorroItems.Clear();
            InfoText.Text = "Завантаження даних з Prozorro...";

            try
            {
                var results = await _processor.ProcessTenderAsync(tenderId, CancellationToken.None);
                foreach (var r in results) ProzorroItems.Add(r);
                InfoText.Text = "Готово. Порівняння цін відображено в таблиці нижче.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Помилка: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                InfoText.Text = "Сталася помилка під час аналізу тендеру.";
            }
            finally
            {
                AnalyzeButton.IsEnabled = true;
            }
        }

        // Keep old public method for compatibility - now it delegates to processor
        public async Task ProcessTenderAsync(string tenderId)
        {
            var results = await _processor.ProcessTenderAsync(tenderId, CancellationToken.None);
            foreach (var r in results) ProzorroItems.Add(r);
        }
    }
}
