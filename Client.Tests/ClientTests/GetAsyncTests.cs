using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Flurl.Http;
using Xunit;

namespace AzureDevOpsRest.Tests.ClientTests
{
    public class GetAsyncTests : IClassFixture<TestConfig>
    {
        private readonly TestConfig _config;

        public GetAsyncTests(TestConfig config) => 
            _config = config;

        [Fact]
        public async Task PrivateProject_Authorized()
        {
            var client = new Client(_config.Token);
            await client.GetAsync(new TestRequest($"{_config.Organization}/_apis/projects"));
        }
        
        [Fact]
        public void PrivateProject_EnumerableRequest_Authorized()
        {
            var client = new Client(_config.Token);
            client
                .GetAsync(new TestEnumerableRequest($"{_config.Organization}/_apis/projects"))
                .ToEnumerable()
                .Should()
                .NotBeEmpty();
        }

        [Fact]
        public async Task PrivateProject_WrongToken_Unauthorized()
        {
            var client = new Client(new string('x', 52));
            var ex = await client
                .Invoking(x => x.GetAsync(new TestRequest($"{_config.Organization}/_apis/projects")))
                .Should()
                .ThrowAsync<FlurlHttpException>();

            ex.Which.Call.HttpStatus.Should().Be(HttpStatusCode.Unauthorized);
        }
        
        [Fact]
        public async Task PrivateProject_NonAuthoritativeInformation_Unauthorized()
        {
            var client = new Client();
            var ex = await client
                .Invoking(x => x.GetAsync(new TestRequest($"{_config.Organization}/_apis/projects")))
                .Should()
                .ThrowAsync<FlurlHttpException>();

            ex.Which.Call.HttpStatus.Should().Be(HttpStatusCode.Unauthorized);
        }
        
        [Fact]
        public async Task PublicProject_EmptyToken()
        {
            var client = new Client();
            await client.GetAsync(new TestRequest($"{_config.Organization}/packer-tasks/_apis/build/builds"));
        }

        [Fact]
        public static void RequestArgumentNull() =>
            FluentActions
                .Invoking(() => new Client().GetAsync((IRequest<object>) null))
                .Should()
                .Throw<ArgumentNullException>();
        
        [Fact]
        public static void EnumerableArgumentNull() =>
            FluentActions
                .Invoking(() => new Client().GetAsync((IEnumerableRequest<object>) null))
                .Should()
                .Throw<ArgumentNullException>();

    }
}