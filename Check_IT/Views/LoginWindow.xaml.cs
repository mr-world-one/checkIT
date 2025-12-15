using Check_IT.Interfaces;
using Check_IT.ViewModels;
using System.Windows;

namespace Check_IT
{
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _vm;
        private readonly IAuthService _authService;

        public LoginWindow(LoginViewModel vm, IAuthService authService)
        {
            _vm = vm;
            _authService = authService;
            InitializeComponent();
            DataContext = _vm;

            _vm.LoginSucceeded += OnLoginSucceeded;
            _vm.LoginFailed += OnLoginFailed;
        }

        private void OnLoginSucceeded(Models.User? user)
        {
            if (user != null)
            {
                _authService.SignIn(user);
            }

            MessageBox.Show(this, $"Login successful. Welcome, {user?.Name}!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            Dispatcher.Invoke(() => { DialogResult = true; Close(); });
        }

        private void OnLoginFailed(string error)
        {
            MessageBox.Show(this, $"Login failed: {error}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // password binding: transfer PasswordBox.Password to ViewModel then trigger command
            _vm.Password = PasswordBox.Password;
            if (_vm.LoginCommand.CanExecute(null)) _vm.LoginCommand.Execute(null);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}