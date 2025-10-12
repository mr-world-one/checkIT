using BCrypt.Net;
using Check_IT.Data;
using Check_IT.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Check_IT.Services
{
    public class UserService
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

        // Added: authenticate user by email + password
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
}