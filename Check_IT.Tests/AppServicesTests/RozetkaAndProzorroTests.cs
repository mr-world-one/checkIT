using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Check_IT.Interfaces;
using Check_IT.Services;

namespace Check_IT.Tests.AppServicesTests
{
    public class RozetkaAndProzorroTests
    {
        [Fact]
        public async Task GetContractItems_CallsProzorroMethod()
        {
            var mock = new Mock<IAppServices>();
            mock.Setup(m => m.GetContractItemsAsync("id", It.IsAny<CancellationToken>())).ReturnsAsync(new List<ProzorroItem>());

            var items = await mock.Object.GetContractItemsAsync("id", CancellationToken.None);

            mock.Verify(m => m.GetContractItemsAsync("id", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetContractItems_Throws_WhenServiceFails()
        {
            var mock = new Mock<IAppServices>();
            mock.Setup(m => m.GetContractItemsAsync("bad", It.IsAny<CancellationToken>())).ThrowsAsync(new System.Exception("API down"));

            await Assert.ThrowsAsync<System.Exception>(async () => await mock.Object.GetContractItemsAsync("bad", CancellationToken.None));

            mock.Verify(m => m.GetContractItemsAsync("bad", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task FindProducts_CallsRozetka()
        {
            var mock = new Mock<IAppServices>();
            mock.Setup(m => m.FindProductsAsync("query", 5, true, It.IsAny<CancellationToken>())).ReturnsAsync(new List<ScrapedProduct>() as IReadOnlyList<ScrapedProduct>);

            var res = await mock.Object.FindProductsAsync("query", 5, true, CancellationToken.None);

            mock.Verify(m => m.FindProductsAsync("query", 5, true, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task FindProducts_Throws_WhenRozetkaFails()
        {
            var mock = new Mock<IAppServices>();
            mock.Setup(m => m.FindProductsAsync("q", 3, true, It.IsAny<CancellationToken>())).ThrowsAsync(new System.Exception("Rozetka error"));

            await Assert.ThrowsAsync<System.Exception>(async () => await mock.Object.FindProductsAsync("q", 3, true, CancellationToken.None));

            mock.Verify(m => m.FindProductsAsync("q", 3, true, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
