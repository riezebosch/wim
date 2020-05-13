using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AzureDevOpsRest.Data;
using Flurl.Http;
using Polly;

namespace AzureDevOpsRest.Requests
{
    public class EnumerableRequest<TData> : IEnumerableRequest<TData>
    {
        public EnumerableRequest(Request<TData> request) =>
            Request = request;

        public IRequest<TData> Request { get; }

        public async IAsyncEnumerable<TData> Enumerator(IFlurlRequest request)
        {
            bool more;
            do
            {
                var task = WithRetry(request);
                more = HandleContinuation(request, await task.ConfigureAwait(false));

                var items = await task.ReceiveJson<Multiple<TData>>();
                foreach (var item in items.Value)
                {
                    yield return item;
                }
            } while (more);
        }

        private static Task<HttpResponseMessage> WithRetry(IFlurlRequest request) => 
            Policy
                .Handle<FlurlHttpException>(ex => ex.Call.HttpStatus >= (HttpStatusCode)500 && ex.Call.HttpStatus <= (HttpStatusCode)599)
                .WaitAndRetryAsync(10, x => TimeSpan.FromSeconds(Math.Pow(2, x)))
                .ExecuteAsync(() => request.GetAsync());

        private static bool HandleContinuation(IFlurlRequest request, HttpResponseMessage response)
        {
            var more = response.Headers.TryGetValues("x-ms-continuationtoken", out var tokens);
            request.SetQueryParam("continuationToken", tokens?.First());
            
            return more;
        }
    }
}