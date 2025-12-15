using System.Threading.Tasks;
using Xunit;
using Moq;
using Check_IT.Interfaces;
using Check_IT.Services;
using System.Threading;
using System.Collections.Generic;

namespace Check_IT.Tests
{
    public class RozetkaScraperTests
    {
        [Fact]
        public async Task FindProductsAsync_ReturnsProducts_WhenMocked()
        {
            var mock = new Mock<IRozetkaScraper>();
            mock.Setup(m => m.FindProductsAsync("Item", 10, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ScrapedProduct> { new ScrapedProduct { Title = "Item A", Price = "100.00", Source = "Rozetka" } } as IReadOnlyList<ScrapedProduct>);

            var res = await mock.Object.FindProductsAsync("Item", 10, true, CancellationToken.None);

            Assert.NotNull(res);
            Assert.Single(res);
            Assert.Equal("Item A", res[0].Title);
            Assert.Equal("100.00", res[0].Price);
            Assert.Equal("Rozetka", res[0].Source);

            mock.Verify(m => m.FindProductsAsync("Item", 10, true, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task FindProductsAsync_Throws_WhenMockedThrows()
        {
            var mock = new Mock<IRozetkaScraper>();
            mock.Setup(m => m.FindProductsAsync("x", 5, true, It.IsAny<CancellationToken>())).ThrowsAsync(new System.Exception("API error"));

            await Assert.ThrowsAsync<System.Exception>(async () => await mock.Object.FindProductsAsync("x", 5, true, CancellationToken.None));

            mock.Verify(m => m.FindProductsAsync("x", 5, true, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
