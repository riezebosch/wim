using System.IO;
using System.Threading.Tasks;
using AzureDevOpsRest;
using Xunit;

namespace MigrateWorkItems.Tests
{
    public class SaveWorkItemsTests
    {
        [Fact]
        public async Task To()
        {
            var config = new TestConfig();
            var client = new Client(config.Token);

            var items = Directory.CreateDirectory(config.Organization).CreateSubdirectory(config.Project).CreateSubdirectory("items");
            await SaveWorkItems.To(client, items, config.Organization, @"SME\Multivers", @"SME\Web based", @"SME\Administrative");
        }
    }
}