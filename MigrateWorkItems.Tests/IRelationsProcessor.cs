using System;
using System.Collections.Generic;
using AzureDevOpsRest.Data.WorkItems;
using Microsoft.AspNetCore.JsonPatch;
using MigrateWorkItems.Tests.Data;

namespace MigrateWorkItems.Tests
{
    internal interface IRelationsProcessor
    {
        void Execute(JsonPatchDocument document, WorkItem item, WorkItemUpdate update, IDictionary<Uri, Uri> mapping);
    }
}