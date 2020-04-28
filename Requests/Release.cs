using System;
using AzureDevOpsRest.Data.Release;

namespace AzureDevOpsRest.Requests
{
    public static class Release
    {
        private class Request<TData> : Requests.Request<TData>
        {
            public Request(string resource, string version) : base(resource, version)
            {
            }

            public override Uri Url => new Uri("https://vsrm.dev.azure.com/");
        }
        
        public static IRequest<Definition> Definition(string organization, string project, int id)
            => new Request<Definition>($"{organization}/{project}/_apis/release/definitions/{id}", "5.1");

        public static IEnumerableRequest<Definition> Definitions(string organization, string project)
            => new Request<Definition>($"{organization}/{project}/_apis/release/definitions", "5.1").AsEnumerable();
    }
}