using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Moq.Protected;
using Check_IT.Services;
using System.Threading;
using System.Text.Json;
using System.Collections.Generic;

namespace Check_IT.Tests.AppServicesTests
{
    public class ProzorroServiceParsingTests
    {
        private HttpClient CreateHttpClientWithResponse(string json, HttpStatusCode status = HttpStatusCode.OK)
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = status,
                   Content = new StringContent(json)
               })
               .Verifiable();

            var client = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new System.Uri("https://public-api.prozorro.gov.ua/api/2.5/")
            };

            return client;
        }

        [Fact]
        public async Task GetContractItemsAsync_ParsesItemsArray()
        {
            var response = JsonSerializer.Serialize(new
            {
                data = new
                {
                    items = new[] {
                        new { description = "Item1", quantity = 2, unit = new { name = "pcs", value = new { amount = 5 } }, value = new { amount = 10 } },
                        new { description = "Item2", quantity = 1, unit = new { name = "kg", value = new { amount = 7 } }, value = new { amount = 7 } }
                    }
                }
            });

            var client = CreateHttpClientWithResponse(response);
            var svc = new ProzorroService(client);

            var items = await svc.GetContractItemsAsync("c1", CancellationToken.None);

            Assert.Equal(2, items.Count);
            Assert.Equal("Item1", items[0].Name);
            Assert.Equal(2m, items[0].Quantity);
            Assert.Equal("pcs", items[0].UnitName);
            Assert.Equal(5m, items[0].UnitPrice);
            Assert.Equal(10m, items[0].TotalPrice);
        }

        [Fact]
        public async Task GetContractItemsAsync_ComputesUnitPrice_WhenSingleItemAndNoUnitPrice()
        {
            var response = JsonSerializer.Serialize(new
            {
                data = new
                {
                    items = new[] {
                        new { description = "OnlyItem", quantity = 4 }
                    },
                    value = new { amount = 40 }
                }
            });

            var client = CreateHttpClientWithResponse(response);
            var svc = new ProzorroService(client);

            var items = await svc.GetContractItemsAsync("c2", CancellationToken.None);

            Assert.Single(items);
            Assert.Equal("OnlyItem", items[0].Name);
            Assert.Equal(4m, items[0].Quantity);
            Assert.Equal(10m, items[0].UnitPrice);
            Assert.Equal(40m, items[0].TotalPrice);
        }

        [Fact]
        public async Task GetContractItemsAsync_Throws_OnApiError()
        {
            var client = CreateHttpClientWithResponse("error", HttpStatusCode.InternalServerError);
            var svc = new ProzorroService(client);

            await Assert.ThrowsAsync<System.Exception>(async () => await svc.GetContractItemsAsync("bad", CancellationToken.None));
        }
    }
}
