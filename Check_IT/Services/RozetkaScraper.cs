using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Check_IT.Services
{
    public class RozetkaScraper
    {
        private readonly HttpClient _http = new HttpClient();
        public string SiteName => "Rozetka";

        public async Task<IReadOnlyList<ScrapedProduct>> FindProductsAsync(string query, int n, bool fastParse, CancellationToken ct)
        {
            try
            {
                string url = $"https://search.rozetka.com.ua/search/api/v6?front-type=xl&text={Uri.EscapeDataString(query)}";

                Console.WriteLine($"[Rozetka API] URL: {url}");

                var response = await _http.GetAsync(url, ct);

                Console.WriteLine($"[Rozetka API] Status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("❌ API error");
                    return Array.Empty<ScrapedProduct>();
                }

                var json = await response.Content.ReadAsStringAsync(ct);

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!root.TryGetProperty("data", out var data))
                    return Array.Empty<ScrapedProduct>();

                if (!data.TryGetProperty("goods", out var goodsArray))
                    return Array.Empty<ScrapedProduct>();


                var results = new List<ScrapedProduct>();

                foreach (var g in goodsArray.EnumerateArray())
                {
                    try
                    {
                        string title = g.GetProperty("title").GetString() ?? "";
                        string link = g.GetProperty("href").GetString() ?? "";

                        decimal price = ExtractPrice(g);

                        if (price <= 0) continue;

                        results.Add(new ScrapedProduct
                        {
                            Source = SiteName,
                            Title = title,
                            Price = price.ToString("0.00"),
                            Url = link
                        });

                        if (results.Count >= n)
                            break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Parsing error: {ex.Message}");
                    }
                }

                Console.WriteLine($"[Rozetka] Found {results.Count} products for '{query}'");

                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RozetkaScraper error: {ex.Message}");
                return Array.Empty<ScrapedProduct>();
            }
        }

        private decimal ExtractPrice(JsonElement g)
        {
            try
            {
                if (g.TryGetProperty("price", out var priceEl))
                {
                    if (priceEl.ValueKind == JsonValueKind.Number)
                    {
                        if (priceEl.TryGetInt32(out int i)) return i;
                        if (priceEl.TryGetInt64(out long l)) return l;
                        if (priceEl.TryGetDecimal(out decimal d)) return d;
                    }
                }

                if (g.TryGetProperty("price_min", out var minEl))
                {
                    if (minEl.TryGetInt32(out int i)) return i;
                    if (minEl.TryGetInt64(out long l)) return l;
                    if (minEl.TryGetDecimal(out decimal d)) return d;
                }

                if (g.TryGetProperty("price_old", out var oldEl))
                {
                    if (oldEl.TryGetInt32(out int i)) return i;
                    if (oldEl.TryGetInt64(out long l)) return l;
                    if (oldEl.TryGetDecimal(out decimal d)) return d;
                }
            }
            catch { }

            return 0;
        }
    }

    public static class JsonNumberExtensions
    {
        public static bool TryGetDecimal(this JsonElement el, out decimal value)
        {
            try { value = el.GetDecimal(); return true; }
            catch { value = 0; return false; }
        }
    }

    public class ScrapedProduct
    {
        public string? Source { get; set; }
        public string? Title { get; set; }
        public string? Price { get; set; }
        public string? Url { get; set; }
    }
}
