using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Check_IT.Interfaces;

namespace Check_IT.Services
{
    public class ZakupivliProService : IZakupivliProService
    {
        private readonly HttpClient _http;

        public ZakupivliProService(HttpClient? httpClient = null)
        {
            _http = httpClient ?? new HttpClient();
        }

        public async Task<List<TenderItem>> LoadContractItemsAsync(string contractId)
        {
            string url = $"https://zakupivli.pro/gov/contracts/{contractId}";

            var json = await _http.GetStringAsync(url);

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var items = new List<TenderItem>();

            if (root.TryGetProperty("contractItems", out var arr) && arr.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in arr.EnumerateArray())
                {
                    decimal? price = null;
                    if (item.TryGetProperty("unitPrice", out var p) && p.ValueKind == JsonValueKind.Number)
                    {
                        if (p.TryGetInt32(out int ii)) price = ii;
                        else if (p.TryGetInt64(out long ll)) price = ll;
                        else if (p.TryGetDecimal(out decimal dd)) price = dd;
                    }

                    items.Add(new TenderItem
                    {
                        Name = item.TryGetProperty("name", out var nameEl) && nameEl.ValueKind == JsonValueKind.String ? nameEl.GetString() : "",
                        Quantity = item.TryGetProperty("quantity", out var qEl) && qEl.ValueKind == JsonValueKind.Number ? qEl.GetDecimal() : 0,
                        Price = price ?? 0
                    });
                }
            }

            return items;
        }
    }

    public class TenderItem
    {
        public string? Name { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
