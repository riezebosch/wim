using System.Threading.Tasks;
using MigrateWorkItems.Tests.Data;

namespace MigrateWorkItems.Tests.FieldsProcessors
{
    internal interface IFieldsProcessor
    {
        Task Execute(WorkItemUpdate update);
    }
}