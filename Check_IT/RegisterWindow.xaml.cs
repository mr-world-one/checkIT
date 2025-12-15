using Check_IT.ViewModels;
using System.Windows;

namespace Check_IT
{
    public partial class RegisterWindow : Window
    {
        private readonly RegisterViewModel _vm;

        public RegisterWindow(RegisterViewModel vm)
        {
            _vm = vm;
            InitializeComponent();
            DataContext = _vm;

            _vm.RegisterSucceeded += OnRegisterSucceeded;
            _vm.RegisterFailed += OnRegisterFailed;
        }

        private void OnRegisterSucceeded()
        {
            MessageBox.Show(this, "Registration successful.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            Dispatcher.Invoke(() => { DialogResult = true; Close(); });
        }

        private void OnRegisterFailed(string error)
        {
            MessageBox.Show(this, $"Registration failed: {error}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.Password = PasswordBox.Password;
            if (_vm.RegisterCommand.CanExecute(null)) _vm.RegisterCommand.Execute(null);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
