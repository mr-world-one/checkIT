using Check_IT.Models;

namespace Check_IT.Interfaces
{
    public interface IAuthService
    {
        User? CurrentUser { get; }
        bool IsAuthenticated { get; }
        event Action<User?>? AuthenticationStateChanged;
        void SignIn(User user);
        void SignOut();
    }
}