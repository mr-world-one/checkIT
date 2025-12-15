using System.Threading.Tasks;
using Xunit;
using Moq;
using Check_IT.Services;
using Check_IT.Models;
using System.Collections.Generic;
using Check_IT.Interfaces;
using System.Threading;

namespace Check_IT.Tests.AppServicesTests
{
    public class PrivateTenderTests
    {
        [Fact]
        public async Task ProcessExcelWithRozetkaAsync_HandlesMissingPrice()
        {
            var mock = new Mock<IRozetkaScraper>();
            mock.Setup(m => m.FindProductsAsync("ItemA", 20, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ScrapedProduct>() as IReadOnlyList<ScrapedProduct>);

            var processor = new PrivateTenderProcessor(mock.Object);
            var items = new List<ComparisonItem> { new ComparisonItem { Name = "ItemA", Price = null } };

            var res = await processor.ProcessAsync(items, mock.Object, CancellationToken.None);

            Assert.Single(res);
            Assert.Null(res[0].RozetkaPrice);
        }
    }
}
