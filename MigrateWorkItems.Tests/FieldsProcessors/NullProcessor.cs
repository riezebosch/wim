using System.Threading.Tasks;
using MigrateWorkItems.Tests.Data;

namespace MigrateWorkItems.Tests.FieldsProcessors
{
    public class NullProcessor : IFieldsProcessor
    {
        public Task Execute(WorkItemUpdate update)
        {
            foreach (var (key, value) in update.Fields)
            {
                if (value.NewValue == null)
                {
                    update.Fields.Remove(key);
                }
            }
            
            return Task.CompletedTask;
        }
    }
}