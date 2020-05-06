using System.Collections.Generic;
using System.Threading.Tasks;
using MigrateWorkItems.Data;

namespace MigrateWorkItems
{
    public interface IFieldsResolver
    {
        Task<IEnumerable<WorkItemField>> ListAllFields();
    }
}