using Check_IT.Interfaces;
using System.Windows.Input;

namespace Check_IT.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly IUserService _userService;

        public RegisterViewModel(IUserService userService)
        {
            _userService = userService;
            RegisterCommand = new RelayCommand(async _ => await RegisterAsync(), _ => CanRegister && !IsBusy);
        }

        private string? _name;
        public string? Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                {
                    RaisePropertyChanged(nameof(CanRegister));
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private string? _email;
        public string? Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                {
                    RaisePropertyChanged(nameof(CanRegister));
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private string? _password;
        public string? Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                {
                    RaisePropertyChanged(nameof(CanRegister));
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    RaisePropertyChanged(nameof(CanRegister));
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private string? _error;
        public string? Error
        {
            get => _error;
            set => SetProperty(ref _error, value);
        }

        public ICommand RegisterCommand { get; }

        public bool CanRegister => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrEmpty(Password);

        public event Action? RegisterSucceeded;
        public event Action<string>? RegisterFailed;

        private async Task RegisterAsync()
        {
            if (!CanRegister || IsBusy) return;
            IsBusy = true;
            Error = null;
            try
            {
                await _userService.CreateUserAsync(Email!, Name!, Password!);
                RegisterSucceeded?.Invoke();
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                RegisterFailed?.Invoke(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
