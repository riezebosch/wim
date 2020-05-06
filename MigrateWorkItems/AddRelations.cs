using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using MigrateWorkItems.Data;

namespace MigrateWorkItems
{
    internal class AddRelations : IRelationsProcessor
    {
        private readonly IMapper _mapper;

        public AddRelations(IMapper mapper) => _mapper = mapper;

        public Task Execute(JsonPatchDocument document, WorkItemUpdate update)
        {
            if (update.Relations?.Added == null) return Task.CompletedTask;
            foreach (var relation in update.Relations.Added)
            {
                if (!relation.Url.ToWorkItemId(out var id)) continue;
                if (!_mapper.TryGetWorkItem(id, out var url)) continue;
                
                relation.Url = url;
                document.Add("/relations/-", relation);
            }
            
            return Task.CompletedTask;
        }
    }
}