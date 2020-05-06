using System.Linq;
using System.Threading.Tasks;
using MigrateWorkItems.Data;

namespace MigrateWorkItems.FieldsProcessors
{
    public class RemoveNotFoundFields : IFieldsProcessor
    {
        private readonly IFieldsResolver _resolver;

        public RemoveNotFoundFields(IFieldsResolver resolver) => _resolver = resolver;

        public async Task Execute(WorkItemUpdate update)
        {
            if (update.Fields == null) return;
            var existing = (await _resolver.ListAllFields()).Select(x => x.ReferenceName).ToHashSet();
            foreach (var (key, _) in update.Fields)
            {
                if (!existing.Contains(key))
                {
                    update.Fields.Remove(key);
                }
            }
        }
    }
}