using System.Collections.Generic;
using System.Threading.Tasks;
using MigrateWorkItems.Data;

namespace MigrateWorkItems.FieldsProcessors
{
    public class RevisedToChangedFields : IFieldsProcessor
    {
        public Task Execute(WorkItemUpdate update)
        {
            if (update.Id > 1)
            {
                update.Fields ??= new Dictionary<string, Value>();
                Replace(update, "System.ChangedBy", update.RevisedBy);
                Replace(update, "System.ChangedDate", update.RevisedDate);
            }

            return Task.CompletedTask;
        }

        private static void Replace(WorkItemUpdate update, string field, object value)
        {
            if (!update.Fields.ContainsKey(field))
            {
                update.Fields[field] = new Value {NewValue = value};
            }
        }
    }
}