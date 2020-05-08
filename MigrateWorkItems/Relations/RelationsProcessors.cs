using System.Threading.Tasks;
using AzureDevOpsRest;
using Microsoft.AspNetCore.JsonPatch;
using MigrateWorkItems.Data;

namespace MigrateWorkItems.Relations
{
    public interface IRelationsProcessors
    {
        Task Execute(JsonPatchDocument document, WorkItemUpdate update);
    }

    public class RelationsProcessors : IRelationsProcessors
    {
        private  readonly IRelationsProcessor[] _processors;

        public RelationsProcessors(IClient client, IMapper mapper)
        {
            _processors = new IRelationsProcessor[]
            {
                new AddWorkItemRelations(mapper), 
                new RemoveWorkItemRelations(client, mapper),
                new AddAttachedFiles(mapper), 
            };
        }

        public async Task Execute(JsonPatchDocument document, WorkItemUpdate update)
        {
            foreach (var processor in _processors)
            {
                await processor.Execute(document, update);
            }
        }
    }
}