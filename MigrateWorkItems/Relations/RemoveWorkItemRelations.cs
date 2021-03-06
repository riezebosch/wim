using System;
using System.Threading.Tasks;
using AzureDevOpsRest;
using AzureDevOpsRest.Data.WorkItems;
using AzureDevOpsRest.Requests;
using Microsoft.AspNetCore.JsonPatch;
using MigrateWorkItems.Data;

namespace MigrateWorkItems.Relations
{
    public class RemoveWorkItemRelations : IRelationsProcessor
    {
        private readonly IClient _client;
        private readonly IMapper _mapper;

        public RemoveWorkItemRelations(IClient client, IMapper mapper)
        {
            _client = client;
            _mapper = mapper;
        }

        public async Task Execute(JsonPatchDocument document, WorkItemUpdate update)
        {
            if (update.Relations?.Removed == null) return;

            var relations = await GetItemRelations(update);
            foreach (var relation in update.Relations.Removed)
            {
                if (!relation.Url.ToWorkItemId(out var id)) continue;
                if (!_mapper.TryGetWorkItem(id, out var url)) continue;

                relation.Url = url;
                Remove(document, relations, relation);
            }
        }

        private static void Remove(JsonPatchDocument document, Relation[] relations, Relation relation)
        {
            var index = Array.FindIndex(relations, x => x.Url == relation.Url && x.Rel == relation.Rel);
            if (index >= 0)
            {
                document.Remove($"/relations/{index}");
            }
        }

        private async Task<Relation[]> GetItemRelations(WorkItemUpdate update)
        {
            if (!_mapper.TryGetWorkItem(update.WorkItemId, out var target)) return Array.Empty<Relation>();

            var item = await _client.GetAsync(new UriRequest<WorkItem>(target, "5.1")
                .WithQueryParams(("$expand", "relations")));
            
            return item.Relations ?? Array.Empty<Relation>();
        }
    }
}