using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Check_IT.Interfaces;
using Check_IT.Models;
using Check_IT.Services;

namespace Check_IT.Services
{
    public class ProzorroProcessor
    {
        private readonly IAppServices? _appServices;
        private readonly ProzorroService _prozorroService;
        private readonly RozetkaScraper _rozetkaScraper;
        private readonly IAppLogger? _logger;

        public ProzorroProcessor(IAppServices? appServices = null, ProzorroService? prozorroService = null, RozetkaScraper? rozetkaScraper = null, IAppLogger? logger = null)
        {
            _appServices = appServices;
            _prozorroService = prozorroService ?? new ProzorroService();
            _rozetkaScraper = rozetkaScraper ?? new RozetkaScraper();
            _logger = logger;
        }

        public async Task<List<ComparisonItem>> ProcessTenderAsync(string tenderId, CancellationToken ct = default)
        {
            _logger?.Information($"Processing tender {tenderId}");

            List<ProzorroItem> items;
            try
            {
                if (_appServices != null)
                    items = await _appServices.GetContractItemsAsync(tenderId, ct);
                else
                    items = await _prozorro_service_getitems(tenderId, ct);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, $"Failed to get contract items for {tenderId}");
                throw;
            }

            var results = new List<ComparisonItem>();
            if (items == null || items.Count == 0)
            {
                _logger?.Warning($"No items found for tender {tenderId}");
                return results;
            }

            foreach (var it in items)
            {
                decimal? rozetkaPrice = null;
                try
                {
                    IReadOnlyList<ScrapedProduct> found;
                    if (_appServices != null)
                        found = await _appServices.FindProductsAsync(it.Name, 20, true, ct);
                    else
                        found = await _rozetka_scrape_cached(it.Name, ct);

                    if (found.Any() && decimal.TryParse(found.First().Price, out var fp))
                        rozetkaPrice = fp;

                    _logger?.Debug($"Item '{it.Name}' - Rozetka price: {rozetkaPrice}");
                }
                catch (Exception ex)
                {
                    _logger?.Warning($"Rozetka lookup failed for '{it.Name}': {ex.Message}");
                    rozetkaPrice = null;
                }

                results.Add(new ComparisonItem
                {
                    Name = it.Name,
                    Price = it.UnitPrice,
                    RozetkaPrice = rozetkaPrice
                });
            }

            _logger?.Information($"Processed tender {tenderId}: {results.Count} comparison items");
            return results;
        }

        // extracted helpers to keep original behavior but allow logging and easier unit testing
        private Task<List<ProzorroItem>> _prozorro_service_getitems(string tenderId, CancellationToken ct)
        {
            return _prozorroService.GetContractItemsAsync(tenderId, ct);
        }

        private Task<IReadOnlyList<ScrapedProduct>> _rozetka_scrape_cached(string q, CancellationToken ct)
        {
            return _rozetkaScraper.FindProductsAsync(q, 20, true, ct);
        }
    }
}