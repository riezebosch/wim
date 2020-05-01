using System.Collections.Generic;
using MigrateWorkItems.Tests.Data;

namespace MigrateWorkItems.Tests.FieldsProcessors
{
    internal class RevisedToChangedFields : IFieldsProcessor
    {
        public void Execute(WorkItemUpdate update)
        {
            update.Fields ??= new Dictionary<string, Value>();
            Replace(update, "System.ChangedBy", update.RevisedBy);
            Replace(update, "System.AuthorizedAs", update.RevisedBy);
            Replace(update, "System.ChangedDate", update.RevisedDate);
            Replace(update, "System.AuthorizedDate", update.RevisedDate);
            Replace(update, "System.RevisedDate", update.RevisedDate);
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