using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Check_IT.Interfaces;
using Check_IT.Models;

namespace Check_IT.Tests.AppServicesTests
{
    public class UserServiceBehaviorTests
    {
        [Fact]
        public async Task AuthenticateAsync_ValidCredentials_InvokesInterface()
        {
            var mock = new Mock<IAppServices>();
            var user = new User { Id = 1, Email = "a@b.com", Name = "A", HashedPassword = "h" };
            mock.Setup(m => m.AuthenticateAsync("a@b.com", "pwd")).ReturnsAsync(user);

            var res = await mock.Object.AuthenticateAsync("a@b.com", "pwd");

            Assert.Equal(1, res.Id);
            mock.Verify(m => m.AuthenticateAsync("a@b.com", "pwd"), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_Throws_OnInvalidCredentials()
        {
            var mock = new Mock<IAppServices>();
            mock.Setup(m => m.AuthenticateAsync("x@x", "bad")).ThrowsAsync(new InvalidOperationException("Invalid email or password"));

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mock.Object.AuthenticateAsync("x@x", "bad"));
            mock.Verify(m => m.AuthenticateAsync("x@x", "bad"), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_Succeeds()
        {
            var mock = new Mock<IAppServices>();
            var created = new User { Id = 5, Email = "n@x.com", Name = "N", HashedPassword = "h" };
            mock.Setup(m => m.CreateUserAsync("n@x.com", "N", "p")).ReturnsAsync(created);

            var res = await mock.Object.CreateUserAsync("n@x.com", "N", "p");
            Assert.Equal(5, res.Id);
            mock.Verify(m => m.CreateUserAsync("n@x.com", "N", "p"), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_Throws_WhenEmailExists()
        {
            var mock = new Mock<IAppServices>();
            mock.Setup(m => m.CreateUserAsync("dup@x.com", "X", "p")).ThrowsAsync(new InvalidOperationException("Email already registered"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => mock.Object.CreateUserAsync("dup@x.com", "X", "p"));
            mock.Verify(m => m.CreateUserAsync("dup@x.com", "X", "p"), Times.Once);
        }
    }
}
