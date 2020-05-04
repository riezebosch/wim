using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;

namespace AzureDevOpsRest
{
    public interface IClient
    {
        Task<TData> GetAsync<TData>(IRequest<TData> request);
        IAsyncEnumerable<TData> GetAsync<TData>(IEnumerableRequest<TData> enumerable);
        Task<TData> PatchAsync<TData>(IRequest<TData> request, JsonPatchDocument content);
        Task<TData> PostAsync<TData>(IRequest<TData> request, object data);
    }
}