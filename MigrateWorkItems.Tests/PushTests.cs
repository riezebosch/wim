using System.IO;
using System.Threading.Tasks;
using AzureDevOpsRest;
using MigrateWorkItems.Model;
using MigrateWorkItems.Relations;
using MigrateWorkItems.Save;
using NSubstitute;
using Xunit;

namespace MigrateWorkItems.Tests
{
    public class WorkItemProcessorTests
    {
        [Fact]
        public async Task Run()
        {
            var config = new TestConfig();
            
            var client = new Client(config.Token);
            var mapper = Substitute.For<IMapper>();
            var processor = new WorkItemProcessor(client, config.Organization, config.Project, new FieldsResolver(client, config.Organization, config.Project), new RelationsProcessors(client, mapper), mapper);
            await processor.Process(config.Organization, config.Project,
                Clone.FromFile(Path.Join("items", "2195", "1.json")));
        }
    }
}