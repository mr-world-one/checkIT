using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Check_IT.Models;
using Check_IT.Services;

namespace Check_IT.Interfaces
{
    public interface IAppServices
    {
        // User service
        Task<User> CreateUserAsync(string email, string name, string password);
        Task<User> AuthenticateAsync(string email, string password);
        Task<User> GetUserAsync(int userId);
        Task DeleteUserAsync(int userId);

        // Rozetka
        Task<IReadOnlyList<ScrapedProduct>> FindProductsAsync(string query, int n, bool fastParse, CancellationToken ct);

        // Prozorro
        Task<List<ProzorroItem>> GetContractItemsAsync(string contractId, CancellationToken ct = default);

        // Excel processing for private tender
        Task<ComparisonItem[]> ProcessExcelWithRozetkaAsync(string filePath);
    }
}