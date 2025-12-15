using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Check_IT.Services;

namespace Check_IT.Interfaces
{
    public interface IRozetkaScraper
    {
        Task<IReadOnlyList<ScrapedProduct>> FindProductsAsync(string query, int n, bool fastParse, CancellationToken ct);
    }
}