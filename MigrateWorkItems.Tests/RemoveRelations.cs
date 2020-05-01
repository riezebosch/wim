using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureDevOpsRest;
using AzureDevOpsRest.Data.WorkItems;
using Microsoft.AspNetCore.JsonPatch;
using MigrateWorkItems.Tests.Data;

namespace MigrateWorkItems.Tests
{
    internal class RemoveRelations : IRelationsProcessor
    {
        private readonly IClient _client;

        public RemoveRelations(IClient client) => _client = client;

        public async Task Execute(JsonPatchDocument document, Uri original, WorkItemUpdate update,
            IDictionary<Uri, Uri> mapping)
        {
            if (update.Relations?.Removed == null) return;

            var item = await _client.GetAsync(new UriRequest<WorkItem>(original, "5.1")
                    .WithQueryParams(("$expand", "relations")));
            if (item.Relations == null) return;
            
            foreach (var relation in update.Relations.Removed)
            {
                if (!mapping.TryGetValue(relation.Url, out var url)) continue;
                
                var index = Array.FindIndex(item.Relations, x => x.Url == url && x.Rel == relation.Rel);
                if (index >= 0)
                {
                    document.Remove($"/relations/{index}");
                }
            }
        }
    }
}