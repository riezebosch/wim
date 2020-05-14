using System;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace AzureDevOpsRest.Requests.Tests
{
    public class TopSkipEnumeratorTests
    {
        [Fact]
        public void Do()
        {
            var config = new TestConfig();
            var client = new Client(config.Token);
            
            var request = new UriRequest<object>(new Uri($"https://dev.azure.com/{config.Organization}/{config.Project}/_apis/wit/workItems/2197/updates"), "5.1")
                .AsTopSkipEnumerable();

            client.GetAsync(request)
                .ToEnumerable()
                .Should()
                .HaveCountGreaterThan(200);
        }
    }
}