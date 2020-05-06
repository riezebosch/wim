using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.AspNetCore.JsonPatch;
using Polly;

namespace AzureDevOpsRest
{
    public class Client : IClient
    {
        private readonly PersonalAccessToken _token;

        public Client(PersonalAccessToken token) => _token = token;

        public Client() : this(PersonalAccessToken.Empty)
        {
        }

        static Client() => 
            FlurlHttp.Configure(settings => settings.HttpClientFactory = new HttpClientFactory());

        public Task<TData> GetAsync<TData>(IRequest<TData> request) =>
            request switch
            {
                null => throw new ArgumentNullException(nameof(request)),
                IRequest<Stream> _ => (Task<TData>) (object) Setup(request).GetStreamAsync(),
                _ => Setup(request).AllowHttpStatus(HttpStatusCode.NotFound).GetJsonAsync<TData>()
            };

        public IAsyncEnumerable<TData> GetAsync<TData>(IEnumerableRequest<TData> enumerable)
        {
            if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));
            return enumerable.Enumerator(Setup(enumerable.Request));
        }

        private IFlurlRequest Setup<TData>(IRequest<TData> request) =>
            new Url(request.Url)
                .AppendPathSegment(request.Resource)
                .WithHeaders(request.Headers)
                .SetQueryParams(request.QueryParams)
                .WithBasicAuth(string.Empty, _token);

        public Task<TData> PostAsync<TData>(IRequest<TData> request, object data)
        {
            if (data is Stream stream)
            {
                return Setup(request)
                    .WithHeader("Content-Type", "application/octet-stream")
                    .PostMultipartAsync(mp => mp.AddFile(Guid.NewGuid().ToString(), stream, ""))
                    .ReceiveJson<TData>();
            }
            return Policy
                .Handle<FlurlHttpException>(ex => ex.Call.HttpStatus == HttpStatusCode.InternalServerError)
                .WaitAndRetryAsync(10, x => TimeSpan.FromSeconds(x * x))
                .ExecuteAsync(() => Setup(request).PostJsonAsync(data).ReceiveJson<TData>());
        }

        public Task<TData> PatchAsync<TData>(IRequest<TData> request, JsonPatchDocument document) =>
            Policy
                .Handle<FlurlHttpException>(ex => ex.Call.HttpStatus == HttpStatusCode.InternalServerError)
                .WaitAndRetryAsync(5, x => TimeSpan.FromSeconds(x * x))
                .ExecuteAsync(() => Setup(request).PatchJsonAsync(document).ReceiveJson<TData>());
    }
}