using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureDevOpsRest
{
    public interface IClient
    {
        Task<TData> GetAsync<TData>(IRequest<TData> request);
        IAsyncEnumerable<TData> GetAsync<TData>(IEnumerableRequest<TData> enumerable);
    }
}