using System.IO;
using System.Threading.Tasks;
using AzureDevOpsRest;
using Xunit;

namespace MigrateWorkItems.Tests
{
    public class SaveWorkItemsTests
    {
        [Fact]
        public async Task Load()
        {
            var config = new TestConfig();
            var client = new Client(config.Token);

            var projectDir = Directory.CreateDirectory(config.Organization).CreateSubdirectory(config.Project);
            await SaveWorkItems.To(client, projectDir, config.Organization, config.Project);
        }
    }
}