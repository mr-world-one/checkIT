using Check_IT.Models;

namespace Check_IT.Interfaces
{
    public interface IUserService
    {
        Task<User> CreateUserAsync(string email, string name, string password);
        Task<User> AuthenticateAsync(string email, string password);
        Task<User> GetUserAsync(int userId);
        Task DeleteUserAsync(int userId);
    }
}
