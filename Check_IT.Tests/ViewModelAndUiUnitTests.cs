using System;
using Xunit;
using Moq;
using Check_IT;
using Check_IT.ViewModels;
using Check_IT.Interfaces;
using Check_IT.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Check_IT.Tests
{
    public class ViewModelAndUiUnitTests
    {
        [Fact]
        public void RelayCommand_Execute_CallsAction()
        {
            bool called = false;
            var cmd = new RelayCommand(_ => called = true);

            Assert.True(cmd.CanExecute(null));
            cmd.Execute(null);
            Assert.True(called);
        }

        [Fact]
        public void RelayCommand_CanExecute_Respected()
        {
            var cmd = new RelayCommand(_ => { }, _ => false);

            Assert.False(cmd.CanExecute(null));
        }

        [Fact]
        public void LoginViewModel_CanLogin_False_WhenEmpty()
        {
            var mock = new Mock<IUserService>();
            var vm = new LoginViewModel(mock.Object);

            vm.Email = "";
            vm.Password = "";

            Assert.False(vm.CanLogin);
        }

        [Fact]
        public void LoginViewModel_CanLogin_True_WhenFilled()
        {
            var mock = new Mock<IUserService>();
            var vm = new LoginViewModel(mock.Object);

            vm.Email = "a@b.com";
            vm.Password = "p";

            Assert.True(vm.CanLogin);
        }

        [Fact]
        public async Task LoginViewModel_LoginSucceeded_EventRaised_OnSuccess()
        {
            var mock = new Mock<IUserService>();
            var user = new User { Id = 1, Email = "a@b.com", Name = "A", HashedPassword = "h" };
            mock.Setup(s => s.AuthenticateAsync("a@b.com", "pwd")).ReturnsAsync(user);

            var vm = new LoginViewModel(mock.Object);
            vm.Email = "a@b.com";
            vm.Password = "pwd";

            User? received = null;
            vm.LoginSucceeded += u => received = u;

            var taskObj = vm.GetType().GetMethod("LoginAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(vm, null);

            if (taskObj is Task t) await t;

            Assert.NotNull(received);
            Assert.Equal(1, received!.Id);
        }

        [Fact]
        public async Task LoginViewModel_LoginFailed_EventRaised_OnFailure()
        {
            var mock = new Mock<IUserService>();
            mock.Setup(s => s.AuthenticateAsync("x@x", "bad")).ThrowsAsync(new InvalidOperationException("Invalid email or password"));

            var vm = new LoginViewModel(mock.Object);
            vm.Email = "x@x";
            vm.Password = "bad";

            string? msg = null;
            vm.LoginFailed += m => msg = m;

            var taskObj = vm.GetType().GetMethod("LoginAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(vm, null);

            if (taskObj is Task t) await t;

            Assert.NotNull(msg);
        }
    }
}
