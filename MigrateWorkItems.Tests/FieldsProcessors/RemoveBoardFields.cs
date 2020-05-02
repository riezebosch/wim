using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MigrateWorkItems.Tests.Data;

namespace MigrateWorkItems.Tests.FieldsProcessors
{
    internal class RemoveAutoFields : IFieldsProcessor
    {
        public Task Execute(WorkItemUpdate update)
        {
            foreach (var field in update.Fields)
            {
                RemoveReadOnlyField(update, field.Key);
            }
            
            return Task.CompletedTask;
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