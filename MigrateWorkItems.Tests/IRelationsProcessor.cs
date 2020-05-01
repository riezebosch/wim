using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureDevOpsRest.Data.WorkItems;
using Microsoft.AspNetCore.JsonPatch;
using MigrateWorkItems.Tests.Data;

namespace MigrateWorkItems.Tests
{
    internal interface IRelationsProcessor
    {
        Task Execute(JsonPatchDocument document, Uri original, WorkItemUpdate update, IDictionary<Uri, Uri> mapping);
    }
}