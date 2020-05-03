using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureDevOpsRest;
using AzureDevOpsRest.Requests;
using MigrateWorkItems.Tests.Data;
using MigrateWorkItems.Tests.FieldsProcessors;

namespace MigrateWorkItems.Tests
{
    internal class RemoveNotFoundFields : IFieldsProcessor
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