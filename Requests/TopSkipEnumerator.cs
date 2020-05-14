using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AzureDevOpsRest.Data;
using Flurl.Http;
using Polly;

namespace AzureDevOpsRest.Requests
{
    public class TopSkipEnumerator<TData> : IEnumerableRequest<TData>
    {
        public TopSkipEnumerator(IRequest<TData> request) => Request = request;

        public IRequest<TData> Request { get; }
        public async IAsyncEnumerable<TData> Enumerator(IFlurlRequest request)
        {
            const int top = 200;
            var skip = 0;
            
            request.SetQueryParam("$top", top);
            bool more;
            do
            {

                var items = await HttpResponseMessageExtensions.ReceiveJson<Multiple<TData>>(WithRetry(request));
                more = items.Count == top;
                
                foreach (var item in items.Value)
                {
                    yield return item;
                }

                request.SetQueryParam("$skip", skip += top);
            } while (more);
        }
        
        private static Task<HttpResponseMessage> WithRetry(IFlurlRequest request) => 
            Policy
                .Handle<FlurlHttpException>(ex => ex.Call.HttpStatus >= (HttpStatusCode)500 && ex.Call.HttpStatus <= (HttpStatusCode)599)
                .WaitAndRetryAsync(10, x => TimeSpan.FromSeconds(Math.Pow(2, x)))
                .ExecuteAsync(() => request.GetAsync());

    }
}