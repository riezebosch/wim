using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureDevOpsRest;
using AzureDevOpsRest.Requests;
using FluentAssertions;
using MigrateWorkItems.Tests.Data;
using NaturalSort.Extension;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace MigrateWorkItems.Tests
{
    public class WorkItemProcessorTests
    {
        [Fact]
        public async Task TestFromJson()
        {
            const string project = "migration-target";
            var config = new TestConfig();
            var client = new Client(config.Token);

            var processor = new WorkItemProcessor(project);
            var itemDir = Path.Join("items", "2038");
            
            var item = await processor.CreateWorkItem(client, config.Organization, project,
                FromFile(Path.Join(itemDir, "1.json")));

            var updates = new[] { 2, 3, 4 }
                .Select(x => Path.Join(itemDir, $"{x}.json"))
                .Select(FromFile);
            
            await processor
                .UpdateWorkItem(client, item, updates);

            var result = await client.GetAsync(WorkItems.WorkItem(config.Organization, item.Id, 
                "System.AreaPath",
                "System.TeamProject", 
                "System.IterationPath", 
                "System.WorkItemType"));
            result.Fields["System.AreaPath"].Should().Be(project);
            result.Fields["System.TeamProject"].Should().Be(project);
            result.Fields["System.IterationPath"].Should().Be(@$"{project}\Sprint 1");
            result.Fields["System.WorkItemType"].Should().Be("Product Backlog Item");
        }
        
        private static WorkItemUpdate FromFile(string file) 
            => JsonConvert.DeserializeObject<WorkItemUpdate>(File.ReadAllText(file), new JsonSerializerSettings {ContractResolver = new CamelCasePropertyNamesContractResolver()});
    }
}