using System.Linq;
using System.Threading.Tasks;
using AzureDevOpsRest.Data.Release;
using FluentAssertions;
using Xunit;

namespace AzureDevOpsRest.Requests.Tests
{
    public class ReleaseTest
    {
        private readonly Client _client = new Client();

        [Fact]
        public async Task Get()
        {
            var result = await _client.GetAsync(Release.Definition("manuel", "packer-tasks", 1));
            result.Should().BeEquivalentTo(new Definition
            {
                Id = 1,
                Name = "marketplace"
            });
        }
        
        [Fact]
        public void List() =>
            _client
                .GetAsync(Release.Definitions("manuel", "packer-tasks"))
                .ToEnumerable()
                .Should()
                .NotBeEmpty();
    }
}