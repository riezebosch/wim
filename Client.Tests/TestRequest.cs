using System;
using System.Collections.Generic;

namespace AzureDevOpsRest.Tests
{
    internal class TestRequest : IRequest<object>
    {
        public TestRequest(string resource) => Resource = resource;

        public string Resource { get; }
        public IDictionary<string, object> QueryParams { get; } = new Dictionary<string, object>();
        public IDictionary<string, object> Headers { get; } = new Dictionary<string, object>();
        public Uri Url => new Uri("https://dev.azure.com/");
    }
}