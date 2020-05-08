using System.IO;
using System.Threading.Tasks;
using Flurl.Http;
using Xunit;
using Xunit.Abstractions;

namespace AzureDevOpsRest.Tests.ClientTests
{
    public class PostAsyncTests : IClassFixture<TestConfig>
    {
        private readonly TestConfig _config;
        private readonly ITestOutputHelper _output;

        public PostAsyncTests(TestConfig config, ITestOutputHelper output)
        {
            _config = config;
            _output = output;
        }

        [Fact]
        public async Task PostStream()
        {
            var client = new Client(_config.Token);
            var request = new TestRequest($"{_config.Organization}/{_config.Project}/_apis/wit/attachments")
                .WithQueryParams(("api-version", "5.1"));

            try
            {
                await using var stream = File.OpenRead("settings.json");
                await client.PostAsync(request, stream);
            }
            catch (FlurlHttpException ex)
            {
                _output.WriteLine(ex.Call.Request.RequestUri.ToString());
                // _output.WriteLine(ex.Call?.RequestBody);

                if (ex.Call.Response?.Content != null)
                {
                    _output.WriteLine(await ex.Call.Response.Content.ReadAsStringAsync());
                }

                throw;
            }
        }
    }
}