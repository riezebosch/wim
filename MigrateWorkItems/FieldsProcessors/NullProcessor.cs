using System.Threading.Tasks;
using MigrateWorkItems.Data;

namespace MigrateWorkItems.FieldsProcessors
{
    public class NullProcessor : IFieldsProcessor
    {
        public Task Execute(WorkItemUpdate update)
        {
            if (update.Fields == null) return Task.CompletedTask;
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