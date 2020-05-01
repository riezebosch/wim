using System.Collections.Generic;
using System.Text.RegularExpressions;
using MigrateWorkItems.Tests.Data;

namespace MigrateWorkItems.Tests.FieldsProcessors
{
    internal class RemoveAutoFields : IFieldsProcessor
    {
        public void Execute(WorkItemUpdate update)
        {
            foreach (var field in update.Fields)
            {
                RemoveReadOnlyField(update, field.Key);
            }
        }

        private static void RemoveReadOnlyField(WorkItemUpdate update, string field)
        {
            if (Regex.IsMatch(field, "^WEF_[A-F0-9]*_(System.ExtensionMarker|Kanban.Column|Kanban.Column.Done)$"))
            {
                update.Fields.Remove(field);
            }
        }
    }
}