using MigrateWorkItems.Tests.Data;

namespace MigrateWorkItems.Tests.FieldsProcessors
{
    internal interface IFieldsProcessor
    {
        void Execute(WorkItemUpdate update);
    }
}