using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using MigrateWorkItems.Data;

namespace MigrateWorkItems.Relations
{
    public class AddAttachedFiles : IRelationsProcessor
    {
        private readonly IMapper _mapper;

        public AddAttachedFiles(IMapper mapper) => _mapper = mapper;

        public Task Execute(JsonPatchDocument document, WorkItemUpdate item)
        {
            if (item?.Relations?.Added == null) return Task.CompletedTask;
            foreach (var relation in item.Relations.Added.Where(x => x.Rel == "AttachedFile"))
            {
                var id = new Guid(relation.Url.Segments.Last());
                if (_mapper.TryGetAttachment(id, out var url))
                {
                    relation.Url = url;
                    document.Add("/relations/-", relation);
                }
            }
            
            return Task.CompletedTask;
        }
    }
}