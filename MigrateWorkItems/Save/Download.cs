using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureDevOpsRest;
using AzureDevOpsRest.Data;
using AzureDevOpsRest.Data.WorkItems;
using AzureDevOpsRest.Requests;
using Newtonsoft.Json.Linq;

namespace MigrateWorkItems.Save
{
    public class Download
    {
        private readonly IClient _client;

        public Download(IClient client) => _client = client;

        public async IAsyncEnumerable<JToken> To(string organization, params string[] areas)
        {
            var tasks = new HashSet<Task<IEnumerable<JToken>>>();
            await foreach (var item in QueryAllWorkItems(organization, areas))
            {
                tasks.Add(DownloadUpdates(item));
                while (tasks.Count > 50)
                {
                    while (tasks.Count > 25)
                    {
                        var task = await Task.WhenAny(tasks);
                        foreach (var update in await task)
                        {
                            yield return update;
                        }

                        tasks.Remove(task);
                    }
                }
            }

            foreach (var update in (await Task.WhenAll(tasks)).SelectMany(x => x))
            {
                yield return update;
            }
        }

        private async Task<IEnumerable<JToken>> DownloadUpdates(WorkItemRef item)
        {
            var updates = new List<JToken>();
            await foreach (var update in _client.GetAsync(
                new UriRequest<JToken>(new Uri(item.Url + "/updates"), "5.1").AsEnumerable()))
            {
                updates.Add(update);
            }

            return updates;
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