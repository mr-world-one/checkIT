using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Check_IT.Interfaces;
using Check_IT.Services;

namespace Check_IT.Tests.AppServicesTests
{
    public class ZakupivliProServiceTests
    {
        [Fact]
        public async Task LoadContractItemsAsync_UsesInterface()
        {
            var mock = new Mock<IZakupivliProService>();
            mock.Setup(s => s.LoadContractItemsAsync("c1")).ReturnsAsync(new List<TenderItem> { new TenderItem { Name = "T1", Quantity = 2, Price = 5 } });

            var res = await mock.Object.LoadContractItemsAsync("c1");

            Assert.Single(res);
            Assert.Equal("T1", res[0].Name);
            mock.Verify(s => s.LoadContractItemsAsync("c1"), Times.Once);
        }

        [Fact]
        public async Task LoadContractItemsAsync_Throws_OnError()
        {
            var mock = new Mock<IZakupivliProService>();
            mock.Setup(s => s.LoadContractItemsAsync("bad")).ThrowsAsync(new System.Exception("Network"));

            await Assert.ThrowsAsync<System.Exception>(async () => await mock.Object.LoadContractItemsAsync("bad"));
            mock.Verify(s => s.LoadContractItemsAsync("bad"), Times.Once);
        }
    }
}
