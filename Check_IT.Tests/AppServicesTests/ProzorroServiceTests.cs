using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Check_IT.Interfaces;
using Check_IT.Services;

namespace Check_IT.Tests.AppServicesTests
{
    public class ProzorroServiceTests
    {
        [Fact]
        public async Task ProcessTenderAsync_SetsRozetkaPriceNull_WhenFindThrows()
        {
            var mock = new Mock<IAppServices>();
            mock.Setup(m => m.GetContractItemsAsync("t2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProzorroItem> { new ProzorroItem { Name = "Y", Quantity = 1, UnitPrice = 5 } });

            mock.Setup(m => m.FindProductsAsync("Y", 20, true, It.IsAny<CancellationToken>())).ThrowsAsync(new System.Exception("Rozetka down"));

            var processor = new ProzorroProcessor(mock.Object);

            var results = await processor.ProcessTenderAsync("t2", CancellationToken.None);

            Assert.Single(results);
            Assert.Equal("Y", results[0].Name);
            Assert.Equal(5m, results[0].Price);
            Assert.Null(results[0].RozetkaPrice);

            mock.Verify(m => m.GetContractItemsAsync("t2", It.IsAny<CancellationToken>()), Times.Once);
            mock.Verify(m => m.FindProductsAsync("Y", 20, true, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
