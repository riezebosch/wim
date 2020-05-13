using System;
using System.Collections.Generic;
using System.Linq;
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
            await foreach (var item in QueryAllWorkItems(organization, areas))
            {
                await foreach (var update in _client.GetAsync(new UriRequest<JToken>(new Uri(item.Url + "/updates"), "5.1").AsEnumerable()))
                {
                    yield return update;
                }
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