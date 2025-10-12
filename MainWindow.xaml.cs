    using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace Check_IT
{
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;

        public MainWindow(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            InitializeComponent();
        }

        private void OpenRegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            var reg = _serviceProvider.GetRequiredService<RegisterWindow>();
            reg.Owner = this;
            reg.ShowDialog();
        }

        private void OpenLoginBtn_Click(object sender, RoutedEventArgs e)
        {
            var login = _serviceProvider.GetRequiredService<LoginWindow>();
            login.Owner = this;
            login.ShowDialog();
        }
    }
}