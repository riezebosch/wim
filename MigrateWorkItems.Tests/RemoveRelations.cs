using System;
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
        private readonly IMapper _mapper;

        public RemoveRelations(IClient client, IMapper mapper)
        {
            _client = client;
            _mapper = mapper;
        }

        public async Task Execute(JsonPatchDocument document, WorkItemUpdate update)
        {
            if (update.Relations?.Removed == null) return;
            if (!_mapper.TryGetWorkItem(update.WorkItemId, out var target)) return;
            
            var item = await _client.GetAsync(new UriRequest<WorkItem>(target, "5.1")
                    .WithQueryParams(("$expand", "relations")));
            if (item.Relations == null) return;
            
            foreach (var relation in update.Relations.Removed)
            {
                if (!relation.Url.ToWorkItemId(out var id)) continue;
                if (!_mapper.TryGetWorkItem(id, out var url)) continue;
                
                var index = Array.FindIndex(item.Relations, x => x.Url == url && x.Rel == relation.Rel);
                if (index >= 0)
                {
                    document.Remove($"/relations/{index}");
                }
            }
        }
    }
}