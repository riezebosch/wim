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

namespace MigrateWorkItems.Tests
{
    internal static class SaveWorkItems
    {
        public static async Task To(Client client, DirectoryInfo target, string organization, params string[] areas)
        {
            var tasks = new List<Task>();
            await foreach (var item in QueryAllWorkItems(client, organization, areas))
            {
                tasks.Add(SaveUpdates(client, item, target));
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
            string[] areas)
        {
            var where = string.Join(" OR ", areas.Select(area => $"[System.AreaPath] UNDER '{area}'"));
            var i = 0;
            while(true)
            {
                var items = await client.PostAsync(WorkItems.Query(organization).WithQueryParams(("$top", 200)),
                    new WorkItemQuery
                    {
                        Query =
                            $"Select [System.Id] FROM WorkItems WHERE ({where}) AND [System.Id] > {i} ORDER BY [System.Id]"
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