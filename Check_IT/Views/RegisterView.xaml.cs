using Check_IT.ViewModels;
using System.Windows;

namespace Check_IT.Views
{
    public partial class RegisterView : System.Windows.Controls.UserControl
    {
        public RegisterView()
        {
            InitializeComponent();

            // ensure DataContext is set from DI if not provided in XAML
            if (DataContext == null)
            {
                var vm = App.AppHost?.Services.GetService(typeof(RegisterViewModel)) as RegisterViewModel;
                if (vm != null)
                    DataContext = vm;
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel vm)
            {
                vm.Password = PasswordBox.Password;
                if (vm.RegisterCommand.CanExecute(null)) vm.RegisterCommand.Execute(null);
            }
        }
    }
}
