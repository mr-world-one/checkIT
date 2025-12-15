using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Check_IT.Services;

namespace Check_IT.Interfaces
{
    public interface IProzorroService
    {
        Task<List<ProzorroItem>> GetContractItemsAsync(string contractId, CancellationToken ct = default);
    }
}