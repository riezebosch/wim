using System;

namespace AzureDevOpsRest.Requests
{
    public class UriRequest<TData> : Request<TData>
    {
        public UriRequest(Uri item, string version) : base(string.Empty, version) => Url = item;

        public override Uri Url { get; }
    }
}