using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using MigrateWorkItems.Data;

namespace MigrateWorkItems
{
    internal interface IRelationsProcessor
    {
        Task Execute(JsonPatchDocument document, WorkItemUpdate update);
    }
}