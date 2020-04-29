using MigrateWorkItems.Tests.Data;

namespace MigrateWorkItems.Tests.FieldsProcessors
{
    internal class ReadOnlyFields : IFieldsProcessor
    {
        private static readonly string[] Fields = { "System.BoardColumn", "System.BoardColumnDone" };

        public void Execute(WorkItemUpdate update)
        {
            foreach (var field in Fields)
            {
                RemoveReadOnlyField(update, field);
            }
        }

        private static void RemoveReadOnlyField(WorkItemUpdate update, string field) => update.Fields.Remove(field);
    }
}