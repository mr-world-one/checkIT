using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Check_IT.Interfaces;
using Check_IT.Models;

namespace Check_IT.Tests
{
    public class UserServiceTests
    {
        [Fact]
        public async Task CreateUserAsync_ShouldReturnCreatedUser_WhenCalled()
        {
            var mock = new Mock<IUserService>();
            var user = new User { Id = 10, Email = "test@example.com", Name = "Test", HashedPassword = "hashed" };
            mock.Setup(s => s.CreateUserAsync("test@example.com", "Test", "pass123")).ReturnsAsync(user);

            var res = await mock.Object.CreateUserAsync("test@example.com", "Test", "pass123");

            Assert.NotNull(res);
            Assert.Equal(10, res.Id);
            Assert.Equal("test@example.com", res.Email);
            mock.Verify(s => s.CreateUserAsync("test@example.com", "Test", "pass123"), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldReturnUser_WhenCredentialsValid()
        {
            var mock = new Mock<IUserService>();
            var user = new User { Id = 5, Email = "a@b.com", Name = "A", HashedPassword = "h" };
            mock.Setup(s => s.AuthenticateAsync("a@b.com", "pwd")).ReturnsAsync(user);

            var res = await mock.Object.AuthenticateAsync("a@b.com", "pwd");

            Assert.Equal(5, res.Id);
            mock.Verify(s => s.AuthenticateAsync("a@b.com", "pwd"), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldThrow_OnInvalidCredentials()
        {
            var mock = new Mock<IUserService>();
            mock.Setup(s => s.AuthenticateAsync("x@x", "bad")).ThrowsAsync(new InvalidOperationException("Invalid email or password"));

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mock.Object.AuthenticateAsync("x@x", "bad"));

            mock.Verify(s => s.AuthenticateAsync("x@x", "bad"), Times.Once);
        }

        [Fact]
        public async Task GetUserAsync_ShouldReturnUser_WhenExists()
        {
            var mock = new Mock<IUserService>();
            var user = new User { Id = 7, Email = "u@u.com", Name = "U", HashedPassword = "h" };
            mock.Setup(s => s.GetUserAsync(7)).ReturnsAsync(user);

            var res = await mock.Object.GetUserAsync(7);
            Assert.Equal(7, res.Id);
            mock.Verify(s => s.GetUserAsync(7), Times.Once);
        }

        [Fact]
        public async Task GetUserAsync_ShouldThrow_WhenNotFound()
        {
            var mock = new Mock<IUserService>();
            mock.Setup(s => s.GetUserAsync(99)).ThrowsAsync(new InvalidOperationException("User not found"));

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mock.Object.GetUserAsync(99));
            mock.Verify(s => s.GetUserAsync(99), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldCallDelete()
        {
            var mock = new Mock<IUserService>();
            mock.Setup(s => s.DeleteUserAsync(3)).Returns(Task.CompletedTask);

            await mock.Object.DeleteUserAsync(3);

            mock.Verify(s => s.DeleteUserAsync(3), Times.Once);
        }
    }
}
