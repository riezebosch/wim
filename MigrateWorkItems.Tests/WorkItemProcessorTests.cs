using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureDevOpsRest;
using AzureDevOpsRest.Data.WorkItems;
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

            var pbi1 = await processor.CreateWorkItem(client, config.Organization, project,
                FromFile(Path.Join(Path.Join("items", "2195"), "1.json")));
            
            var task = await processor.CreateWorkItem(client, config.Organization, project,
                FromFile(Path.Join("items", "2196", "1.json")));

            var pbi2 = await processor.CreateWorkItem(client, config.Organization, project,
                FromFile(Path.Join(Path.Join("items", "2197"), "1.json")));
            
            var mapping = new Dictionary<Uri, Uri>
            {
                [new Uri("https://dev.azure.com/manuel/eb082604-a70f-4977-9335-85f0da463818/_apis/wit/workItems/2195")] = pbi1.Url,
                [new Uri("https://dev.azure.com/manuel/eb082604-a70f-4977-9335-85f0da463818/_apis/wit/workItems/2196")] = task.Url,
                [new Uri("https://dev.azure.com/manuel/eb082604-a70f-4977-9335-85f0da463818/_apis/wit/workItems/2197")] = pbi2.Url,
            };

            var updates = 
                LoadUpdates(pbi1, "2195", 2, 3, 4)
                    .Concat(LoadUpdates(task, "2196", 2, 3))
                    .Concat(LoadUpdates(pbi2, "2197", 2, 3, 4, 5));

            foreach (var (item, update) in updates
                .OrderBy(x => x.update.RevisedDate.Year != 9999 ? x.update.RevisedDate : x.update.Fields["System.ChangedDate"].NewValue)
                .ThenByDescending(x => x.update.Fields?.Count + x.update.Relations?.Added?.Count() + x.update.Relations?.Removed?.Count()))
            {
                await processor.UpdateWorkItem(client, item.Url, update, mapping);
            }

            var result = await client.GetAsync(WorkItems.WorkItem(config.Organization, pbi1.Id, 
                "System.AreaPath",
                "System.TeamProject", 
                "System.IterationPath", 
                "System.WorkItemType"));
            result.Fields["System.AreaPath"].Should().Be(project);
            result.Fields["System.TeamProject"].Should().Be(project);
            result.Fields["System.IterationPath"].Should().Be(@$"{project}\Sprint 1");
            result.Fields["System.WorkItemType"].Should().Be("Product Backlog Item");
            
            var child = await client.GetAsync(WorkItems.WorkItem(config.Organization, task.Id, 
                "System.Parent"));
            child.Fields["System.Parent"].Should().Be(pbi2.Id);
        }

        private static IEnumerable<(WorkItem item, WorkItemUpdate update)> LoadUpdates(WorkItem item, string id, params int[] updates)
        {
            return updates
                .Select(x => Path.Join("items", id, $"{x}.json"))
                .Select(FromFile)
                .Select(x => (item, x));
        }

        private static WorkItemUpdate FromFile(string file) 
            => JsonConvert.DeserializeObject<WorkItemUpdate>(File.ReadAllText(file), new JsonSerializerSettings {ContractResolver = new CamelCasePropertyNamesContractResolver()});

        [Fact]
        public void LoadOrder()
        {
            LoadUpdates(null, "2195", 2, 3, 4).OrderBy(x => x.update.RevisedDate.Year != 9999 ? x.update.RevisedDate : x.update.Fields["System.ChangedDate"].NewValue)
                .Select(x => x.update.Id)
                .Should()
                .BeEquivalentTo(new[] { 2, 3, 4 }, options => options.WithStrictOrdering());
            
            LoadUpdates(null, "2196", 2, 3).OrderBy(x => x.update.RevisedDate)
                .Select(x => x.update.Id)
                .Should()
                .BeEquivalentTo(new[] { 2, 3 }, options => options.WithStrictOrdering());
            
            LoadUpdates(null, "2197", 2, 3, 4, 5).OrderBy(x => x.update.RevisedDate)
                .Select(x => x.update.Id)
                .Should()
                .BeEquivalentTo(new[] { 2, 3, 4, 5 }, options => options.WithStrictOrdering());
        }
    }
}