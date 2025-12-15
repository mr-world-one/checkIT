using Check_IT.Interfaces;
using Check_IT.Models;

namespace Check_IT.Services
{
    public class AuthService : IAuthService
    {
        private User? _currentUser;
        public User? CurrentUser => _currentUser;
        public bool IsAuthenticated => _currentUser != null;
        public event Action<User?>? AuthenticationStateChanged;

        public void SignIn(User user)
        {
            _currentUser = user;
            AuthenticationStateChanged?.Invoke(_currentUser);
        }

        public void SignOut()
        {
            _currentUser = null;
            AuthenticationStateChanged?.Invoke(null);
        }
    }
}