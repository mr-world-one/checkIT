using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace Check_IT
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();

            HeaderText.Text = "Про нас";
            IntroText.Text = "Ласкаво просимо на check IT - сервіс, який допомагає порівнювати ціни та умови тендерів для бізнесу та державних закупівель.";
            BodyText.Text = "Наша платформа створена для спрощення аналізу тендерних пропозицій. Ми надаємо можливість швидко знаходити та порівнювати ціни, умови та інші ключові параметри як державних, так і приватних тендерів. Забудьте про нескінченне блукання між різними сайтами - вся необхідна інформація зібрана в одному місці!";

            FeaturesHeader.Text = "Наші можливості:";
            Feature1.Text = "- Аналіз державних тендерних пропозицій та закупівельних угод через Prozorro";
            Feature2.Text = "- Порівняння умов приватних тендерів для бізнесу";
            Feature3.Text = "- Зручний інтерфейс для швидкого доступу до актуальної інформації";

            ContactsHeader.Text = "Контакти:";
            Contact1.Text = "+380 67 755 7973";
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            }
            catch
            {
                MessageBox.Show(this, $"Cannot open {e.Uri}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}