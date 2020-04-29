using System;
using AzureDevOpsRest.Requests;

namespace MigrateWorkItems.Tests
{
    internal class UriRequest<TData> : Request<TData>
    {
        public UriRequest(Uri item, string version) : base(string.Empty, version) => Url = item;

        public override Uri Url { get; }
    }
}