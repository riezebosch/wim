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

namespace MigrateWorkItems
{
    public class SaveWorkItems
    {
        private readonly IClient _client;

        public SaveWorkItems(IClient client) => _client = client;

        public async IAsyncEnumerable<IAsyncEnumerable<JToken>> To(DirectoryInfo target, string organization, DirectoryInfo attachments, params string[] areas)
        {
            await foreach (var item in QueryAllWorkItems(organization, areas))
            {
                yield return SaveUpdates(_client, item, target);
            }
        }
        
        private static async IAsyncEnumerable<JToken> SaveUpdates(IClient client, WorkItemRef item, DirectoryInfo items)
        {
            var itemDir = items.CreateSubdirectory(item.Id.ToString());
            await foreach (var update in client.GetAsync(
                new UriRequest<JToken>(new Uri(item.Url + "/updates"), "5.1").AsEnumerable()))
            {
                var path = Path.Combine(itemDir.FullName, update.SelectToken("id").Value<string>() + ".json");
                File.WriteAllText(path, update.ToString());

                yield return update;
            }
        }

        private async IAsyncEnumerable<WorkItemRef> QueryAllWorkItems(string organization, IEnumerable<string> areas)
        {
            var where = string.Join(" OR ", areas.Select(area => $"[System.AreaPath] UNDER '{area}'"));
            var i = 0;
            
            while(true)
            {
                var items = await _client.PostAsync(WorkItems.Query(organization).WithQueryParams(("$top", 200)),
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