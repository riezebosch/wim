using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using MigrateWorkItems.Tests.Data;

namespace MigrateWorkItems.Tests
{
    internal interface IRelationsProcessor
    {
        Task Execute(JsonPatchDocument document, WorkItemUpdate update);
    }
}