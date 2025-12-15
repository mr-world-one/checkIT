using Check_IT.Interfaces;
using Check_IT.Models;
using System.Windows.Input;

namespace Check_IT.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IUserService _userService;

        public LoginViewModel(IUserService userService)
        {
            _userService = userService;
            LoginCommand = new RelayCommand(async _ => await LoginAsync(), _ => CanLogin);
        }

        private string? _email;
        public string? Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string? _password;
        public string? Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public ICommand LoginCommand { get; }

        public bool CanLogin => !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrEmpty(Password) && !IsBusy;

        public event Action<User?>? LoginSucceeded;
        public event Action<string>? LoginFailed;

        private async Task LoginAsync()
        {
            if (!CanLogin) return;
            IsBusy = true;
            try
            {
                var user = await _userService.AuthenticateAsync(Email!, Password!);
                LoginSucceeded?.Invoke(user);
            }
            catch (Exception ex)
            {
                LoginFailed?.Invoke(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
