using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AzureDevOpsRest.Data.Build;
using FluentAssertions;
using Flurl.Http;
using Xunit;

namespace AzureDevOpsRest.Requests.Tests
{
    public class BuildTests
    {
        private readonly Client _client = new Client();

        [Fact]
        public async Task Definition()
        {
            var result = await _client.GetAsync(Build.Definition("manuel", "packer-tasks", 27));
            result.Should().BeEquivalentTo(new Definition
            {
                Id = 27,
                Name = "riezebosch.vsts-tasks-packer"
            });
        }
        
        [Fact]
        public void Definitions() =>
            _client
                .GetAsync(Build.Definitions("manuel", "packer-tasks"))
                .ToEnumerable()
                .Should()
                .NotBeEmpty();

        [Fact]
        public void Continuation() =>
            _client
                .GetAsync(Build.Builds("manuel", "packer-tasks").WithQueryParams(("$top", 2)))
                .ToEnumerable()
                .Count()
                .Should()
                .BeGreaterThan(4);

        [Fact]
        public async Task InvalidApiVersion_BadRequest()
        {
            var ex = await _client
                .Invoking(x => x.GetAsync(Build.Definition("manuel", "packer-tasks", 1).WithQueryParams(("api-version", "89"))))
                .Should()
                .ThrowAsync<FlurlHttpException>();

            ex.Which.Call.HttpStatus.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}