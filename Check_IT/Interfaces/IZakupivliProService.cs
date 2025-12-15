using System.Collections.Generic;
using System.Threading.Tasks;
using Check_IT.Services;

namespace Check_IT.Interfaces
{
    public interface IZakupivliProService
    {
        Task<List<TenderItem>> LoadContractItemsAsync(string contractId);
    }
}