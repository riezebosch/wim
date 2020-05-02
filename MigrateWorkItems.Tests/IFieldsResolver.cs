using System.Collections.Generic;
using System.Threading.Tasks;
using MigrateWorkItems.Tests.Data;

namespace MigrateWorkItems.Tests
{
    public interface IFieldsResolver
    {
        Task<IEnumerable<WorkItemField>> ListAllFields();
    }
}