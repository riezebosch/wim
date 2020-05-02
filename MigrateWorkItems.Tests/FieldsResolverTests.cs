using System.Linq;
using System.Threading.Tasks;
using AzureDevOpsRest;
using FluentAssertions;
using Xunit;

namespace MigrateWorkItems.Tests
{
    public class FieldsResolverTests
    {
        [Fact]
        public async Task LoadAllFields()
        {
            var config = new TestConfig();
            var client = new Client(config.Token);

            var processor = new FieldsResolver(client, config.Organization, config.Project);
            var fields = await processor.ListAllFields();
            fields
                .Select(f => f.ReferenceName)
                .Distinct()
                .Should()
                .NotBeEmpty();
        }
    }
}