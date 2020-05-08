using System;
using System.Threading.Tasks;
using AzureDevOpsRest;
using AzureDevOpsRest.Data.WorkItems;
using Microsoft.AspNetCore.JsonPatch;
using MigrateWorkItems.Data;

namespace MigrateWorkItems
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
            var relations = await GetItemRelations(update);
            foreach (var relation in update.Relations.Removed)
            {
                if (!relation.Url.ToWorkItemId(out var id)) continue;
                if (!_mapper.TryGetWorkItem(id, out var url)) continue;
                
                var index = Array.FindIndex(relations, x => x.Url == url && x.Rel == relation.Rel);
                if (index >= 0)
                {
                    document.Remove($"/relations/{index}");
                }
            }
        }

        private async Task<Relation[]> GetItemRelations(WorkItemUpdate update)
        {
            if (update.Relations?.Removed == null ||
                !_mapper.TryGetWorkItem(update.WorkItemId, out var target)) return Array.Empty<Relation>();

            var item = await _client.GetAsync(new UriRequest<WorkItem>(target, "5.1")
                .WithQueryParams(("$expand", "relations")));
            
            return item.Relations ?? Array.Empty<Relation>();
        }
    }
}