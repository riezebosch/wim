using System.Threading.Tasks;
using MigrateWorkItems.Data;

namespace MigrateWorkItems.FieldsProcessors
{
    internal interface IFieldsProcessor
    {
        Task Execute(WorkItemUpdate update);
    }
}