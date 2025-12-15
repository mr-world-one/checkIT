using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Check_IT.Services
{
    public class ProzorroItem
    {
        public string Name { get; set; } = "";
        public decimal Quantity { get; set; }
        public string UnitName { get; set; } = "";
        public decimal? UnitPrice { get; set; }      // ціна за одиницю
        public decimal? TotalPrice { get; set; }     // загальна сума по позиції / всьому контракту
    }

    public class ProzorroService
    {
        private readonly HttpClient _http;

        public ProzorroService(HttpClient? httpClient = null)
        {
            _http = httpClient ?? new HttpClient { BaseAddress = new Uri("https://public-api.prozorro.gov.ua/api/2.5/") };
        }

        public async Task<List<ProzorroItem>> GetContractItemsAsync(string contractId, CancellationToken ct = default)
        {
            // 1. Пробуємо /tenders/{id}, якщо 404 – /contracts/{id}
            JsonElement data = await GetTenderOrContractDataAsync(contractId, ct);

            var items = new List<ProzorroItem>();

            if (data.TryGetProperty("items", out var itemsJson) &&
                itemsJson.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in itemsJson.EnumerateArray())
                {
                    ct.ThrowIfCancellationRequested();

                    var resultItem = new ProzorroItem();

                    // description
                    if (item.TryGetProperty("description", out var descEl) &&
                        descEl.ValueKind == JsonValueKind.String)
                    {
                        resultItem.Name = descEl.GetString() ?? "";
                    }

                    // quantity
                    if (item.TryGetProperty("quantity", out var qtyEl) &&
                        qtyEl.ValueKind == JsonValueKind.Number)
                    {
                        resultItem.Quantity = qtyEl.GetDecimal();
                    }

                    // unit.name
                    if (item.TryGetProperty("unit", out var unitEl) &&
                        unitEl.ValueKind == JsonValueKind.Object &&
                        unitEl.TryGetProperty("name", out var unitNameEl) &&
                        unitNameEl.ValueKind == JsonValueKind.String)
                    {
                        resultItem.UnitName = unitNameEl.GetString() ?? "";
                    }

                    // unit.value.amount  -> UnitPrice
                    if (item.TryGetProperty("unit", out unitEl) &&
                        unitEl.ValueKind == JsonValueKind.Object &&
                        unitEl.TryGetProperty("value", out var valueEl) &&
                        valueEl.ValueKind == JsonValueKind.Object &&
                        valueEl.TryGetProperty("amount", out var amountEl) &&
                        amountEl.ValueKind == JsonValueKind.Number)
                    {
                        resultItem.UnitPrice = amountEl.GetDecimal();
                    }

                    // value.amount (загальна сума по позиції, якщо є)
                    if (item.TryGetProperty("value", out var itemValueEl) &&
                        itemValueEl.ValueKind == JsonValueKind.Object &&
                        itemValueEl.TryGetProperty("amount", out var itemTotalEl) &&
                        itemTotalEl.ValueKind == JsonValueKind.Number)
                    {
                        resultItem.TotalPrice = itemTotalEl.GetDecimal();
                    }

                    items.Add(resultItem);
                }
            }

            // 2. Якщо лише одна позиція і в неї немає UnitPrice – пробуємо порахувати з contract.value.amount
            if (items.Count == 1 && (!items[0].UnitPrice.HasValue || items[0].UnitPrice == 0))
            {
                decimal? totalContract = null;

                if (data.TryGetProperty("value", out var contractValueEl) &&
                    contractValueEl.ValueKind == JsonValueKind.Object &&
                    contractValueEl.TryGetProperty("amount", out var totalAmountEl) &&
                    totalAmountEl.ValueKind == JsonValueKind.Number)
                {
                    totalContract = totalAmountEl.GetDecimal();
                }

                if (totalContract.HasValue && items[0].Quantity > 0)
                {
                    var t = items[0];
                    var unitPrice = totalContract.Value / t.Quantity;
                    t.UnitPrice = unitPrice;
                    t.TotalPrice = totalContract;
                }
            }

            return items;
        }

        private async Task<JsonElement> GetTenderOrContractDataAsync(string id, CancellationToken ct)
        {
            // /tenders/{id}
            var resp = await _http.GetAsync($"tenders/{id}", ct);
            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                // /contracts/{id}
                resp = await _http.GetAsync($"contracts/{id}", ct);
            }

            if (!resp.IsSuccessStatusCode)
            {
                var text = await resp.Content.ReadAsStringAsync(ct);
                throw new Exception($"Prozorro API error: {resp.StatusCode}\n{text}");
            }

            var json = await resp.Content.ReadAsStringAsync(ct);

            // БЕЗ using, і головне — Clone()
            var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("data", out var data))
                throw new Exception("Некоректна відповідь Prozorro (немає поля 'data').");

            // Робимо копію, яка НЕ залежить від doc
            return data.Clone();
        }

    }
}
