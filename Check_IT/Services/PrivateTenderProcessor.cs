using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Check_IT.Interfaces;
using Check_IT.Models;

namespace Check_IT.Services
{
    public class PrivateTenderProcessor
    {
        private readonly IRozetkaScraper? _scraper;
        private readonly IAppLogger? _logger;

        public PrivateTenderProcessor(IRozetkaScraper? scraper = null, IAppLogger? logger = null)
        {
            _scraper = scraper;
            _logger = logger;
        }

        public async Task<List<ComparisonItem>> ProcessAsync(IEnumerable<ComparisonItem> items, IRozetkaScraper? scraper = null, CancellationToken ct = default)
        {
            // create a delegate that abstracts the call to find products
            Func<string, int, bool, CancellationToken, Task<IReadOnlyList<ScrapedProduct>>> findFunc;

            if (scraper != null)
            {
                findFunc = (q, n, f, c) => scraper.FindProductsAsync(q, n, f, c);
            }
            else if (_scraper != null)
            {
                findFunc = (q, n, f, c) => _scraper.FindProductsAsync(q, n, f, c);
            }
            else
            {
                findFunc = (q, n, f, c) => new RozetkaScraper().FindProductsAsync(q, n, f, c);
            }

            var list = (items ?? Array.Empty<ComparisonItem>())
                .Select(i => new ComparisonItem { Name = i.Name, Price = i.Price, RozetkaPrice = null })
                .ToList();

            _logger?.Information($"Processing {list.Count} private tender items");

            for (int idx = 0; idx < list.Count; idx++)
            {
                var item = list[idx];
                try
                {
                    var found = await findFunc(item.Name ?? string.Empty, 20, true, ct);
                    if (found != null && found.Any())
                    {
                        var first = found.FirstOrDefault();
                        if (first != null && !string.IsNullOrEmpty(first.Price) && decimal.TryParse(first.Price, out var p))
                        {
                            item.RozetkaPrice = p;
                        }
                    }

                    _logger?.Debug($"Item '{item.Name}' matched with price {item.RozetkaPrice}");
                }
                catch (Exception ex)
                {
                    _logger?.Warning($"Error searching Rozetka for '{item.Name}': {ex.Message}");
                    item.RozetkaPrice = null;
                }
            }

            _logger?.Information($"Finished processing private tender items");
            return list;
        }
    }
}