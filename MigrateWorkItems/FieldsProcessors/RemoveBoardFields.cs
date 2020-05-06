using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MigrateWorkItems.Data;

namespace MigrateWorkItems.FieldsProcessors
{
    public class RemoveAutoFields : IFieldsProcessor
    {
        public Task Execute(WorkItemUpdate update)
        {
            if (update.Fields == null) return Task.CompletedTask;
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