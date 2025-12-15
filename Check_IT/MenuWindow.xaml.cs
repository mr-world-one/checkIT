using Check_IT.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Windows;

namespace Check_IT
{
    public partial class MenuWindow : Window
    {
        public MenuWindow()
        {
            InitializeComponent();
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var about = new AboutWindow();
            about.Owner = this;
            about.ShowDialog();
        }

        private void Website_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("https://example.com") { UseShellExecute = true });
            }
            catch
            {
                MessageBox.Show(this, "Cannot open website.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool EnsureAuthenticated()
        {
            var auth = App.AppHost?.Services.GetService<IAuthService>();
            if (auth != null && auth.IsAuthenticated) return true;

            // show login window from a scope
            var scope = App.AppHost.Services.CreateScope();
            try
            {
                var login = scope.ServiceProvider.GetRequiredService<LoginWindow>();
                login.Owner = this.Owner ?? this;
                var res = login.ShowDialog();
                return auth != null && auth.IsAuthenticated;
            }
            finally
            {
                scope.Dispose();
            }
        }

        private void AnalyzePrivate_Click(object sender, RoutedEventArgs e)
        {
            if (!EnsureAuthenticated()) return;

            var wnd = new PrivateTenderWindow();
            wnd.Owner = this.Owner ?? this;
            wnd.ShowDialog();
        }

        private void AnalyzePublic_Click(object sender, RoutedEventArgs e)
        {
            if (!EnsureAuthenticated()) return;

            var wnd = new ProzorroWindow();
            wnd.Owner = this.Owner ?? this;
            wnd.ShowDialog();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var scope = App.AppHost.Services.CreateScope();
            try
            {
                var login = scope.ServiceProvider.GetRequiredService<LoginWindow>();
                login.Owner = Owner;
                login.ShowDialog();
            }
            finally
            {
                scope.Dispose();
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var auth = App.AppHost?.Services.GetService<IAuthService>();
            auth?.SignOut();
            MessageBox.Show(this, "Logged out.", "Logout", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SignUp_Click(object sender, RoutedEventArgs e)
        {
            var scope = App.AppHost.Services.CreateScope();
            try
            {
                var reg = scope.ServiceProvider.GetRequiredService<RegisterWindow>();
                reg.Owner = Owner;
                reg.ShowDialog();
            }
            finally
            {
                scope.Dispose();
            }
        }
    }
}
