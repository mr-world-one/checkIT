using BCrypt.Net;
using Check_IT.Data;
using Check_IT.Interfaces;
using Check_IT.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using ClosedXML.Excel;
using System.Linq;

namespace Check_IT.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _dbContext;

        public UserService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User> CreateUserAsync(string email, string name, string password)
        {
            var existingUser = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Email already registered");
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User
            {
                Email = email,
                Name = name,
                HashedPassword = hashedPassword,
                IsActive = true
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            return user;
        }

        public async Task<User> GetUserAsync(int userId)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }
            return user;
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<User> AuthenticateAsync(string email, string password)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                throw new InvalidOperationException("Invalid email or password");

            var verified = BCrypt.Net.BCrypt.Verify(password, user.HashedPassword);
            if (!verified)
                throw new InvalidOperationException("Invalid email or password");

            if (!user.IsActive)
                throw new InvalidOperationException("User is not active");

            return user;
        }
    }

    // Adapter to expose UserService through IAppServices
    public class AppServicesAdapter : IAppServices
    {
        private readonly UserService _userService;
        private readonly RozetkaScraper _rozetka;
        private readonly ProzorroService _prozorro;

        public AppServicesAdapter(UserService userService, RozetkaScraper rozetka, ProzorroService prozorro)
        {
            _userService = userService;
            _rozetka = rozetka;
            _prozorro = prozorro;
        }

        public Task<User> CreateUserAsync(string email, string name, string password) => _userService.CreateUserAsync(email, name, password);
        public Task<User> AuthenticateAsync(string email, string password) => _userService.AuthenticateAsync(email, password);
        public Task<User> GetUserAsync(int userId) => _userService.GetUserAsync(userId);
        public Task DeleteUserAsync(int userId) => _userService.DeleteUserAsync(userId);

        public Task<IReadOnlyList<ScrapedProduct>> FindProductsAsync(string query, int n, bool fastParse, System.Threading.CancellationToken ct)
            => _rozetka.FindProductsAsync(query, n, fastParse, ct);

        public Task<System.Collections.Generic.List<ProzorroItem>> GetContractItemsAsync(string contractId, System.Threading.CancellationToken ct = default)
            => _prozorro.GetContractItemsAsync(contractId, ct);

        public async Task<ComparisonItem[]> ProcessExcelWithRozetkaAsync(string filePath)
        {
            var products = new List<ComparisonItem>();

            using (var workbook = new XLWorkbook(filePath))
            {
                var ws = workbook.Worksheets.First();
                foreach (var row in ws.RowsUsed().Skip(1))
                {
                    var name = row.Cell(1).GetString();
                    if (string.IsNullOrWhiteSpace(name)) continue;

                    decimal? excelPrice = null;
                    if (decimal.TryParse(row.Cell(2).GetString().Replace(',', '.'), out var parsed))
                        excelPrice = parsed;

                    products.Add(new ComparisonItem { Name = name, Price = excelPrice });
                }
            }

            for (int i = 0; i < products.Count; i++)
            {
                var item = products[i];
                try
                {
                    var found = await _rozetka.FindProductsAsync(item.Name ?? string.Empty, 10, true, System.Threading.CancellationToken.None);
                    if (found != null && found.Any())
                    {
                        var first = found.FirstOrDefault();
                        if (first != null && decimal.TryParse(first.Price, out var firstPrice))
                            item.RozetkaPrice = firstPrice;
                    }
                }
                catch
                {
                    item.RozetkaPrice = null;
                }
            }

            return products.ToArray();
        }
    }
}