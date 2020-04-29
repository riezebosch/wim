using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureDevOpsRest;
using AzureDevOpsRest.Data;
using AzureDevOpsRest.Data.WorkItems;
using AzureDevOpsRest.Requests;
using Newtonsoft.Json.Linq;
using Xunit;

namespace MigrateWorkItems.Tests
{
    public class LoadAllWorkItems
    {
        [Fact]
        public async Task Load()
        {
            var config = new TestConfig();
            var client = new Client(config.Token);

            var projectDir = Directory.CreateDirectory(config.Organization).CreateSubdirectory(config.Project);
            var tasks = new List<Task>();
            
            await foreach (var item in QueryAllWorkItems(client, config.Organization, config.Project))
            {
                tasks.Add(SaveUpdates(client, item, projectDir));
            }

            await Task.WhenAll(tasks);
        }

        private static async Task SaveUpdates(Client client, WorkItemRef item, DirectoryInfo projectDir)
        {
            var itemDir = projectDir.CreateSubdirectory(item.Id.ToString());
            await foreach (var update in client.GetAsync(
                new UriRequest<JToken>(new Uri(item.Url + "/updates"), "5.1").AsEnumerable()))
            {
                var path = Path.ChangeExtension(Path.Combine(itemDir.FullName, update.SelectToken("id").Value<string>()),
                    "json");
                File.WriteAllText(path, update.ToString());
            }
        }

        private static async IAsyncEnumerable<WorkItemRef> QueryAllWorkItems(Client client, string organization,
            string project)
        {
            var i = 0;
            while(true)
            {
                var items = await client.PostAsync(WorkItems.Query(organization).WithQueryParams(("$top", 200)),
                    new WorkItemQuery
                    {
                        Query =
                            $"Select [System.Id] FROM WorkItems WHERE [System.TeamProject] == '{project}' AND [System.Id] > {i} ORDER BY [System.Id]"
                    });
                
                if (!items.WorkItems.Any())
                {
                    break;
                }

                foreach (var item in items.WorkItems)
                {
                    yield return item;
                    i = item.Id;
                }
            }
        }
    }
}