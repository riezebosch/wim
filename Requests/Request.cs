using System;
using System.Collections.Generic;

namespace AzureDevOpsRest.Requests
{
    public class Request<TData> : IRequest<TData>
    {
        public IDictionary<string, object> QueryParams { get; } = new Dictionary<string, object>();
        public IDictionary<string, object> Headers { get; } = new Dictionary<string, object>();

        public Request(string resource, string version)
        {
            Resource = resource;
            QueryParams["api-version"] = version;
        }

        public string Resource { get; }

        public virtual Uri Url => new Uri("https://dev.azure.com/");

        public IEnumerableRequest<TData> AsEnumerable() => new EnumerableRequest<TData>(this);
    }
}