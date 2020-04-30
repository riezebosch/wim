using System;
using System.Collections.Generic;
using AzureDevOpsRest.Data.WorkItems;
using Microsoft.AspNetCore.JsonPatch;
using MigrateWorkItems.Tests.Data;

namespace MigrateWorkItems.Tests
{
    internal class AddRelations : IRelationsProcessor
    {
        public void Execute(JsonPatchDocument document, WorkItem item, WorkItemUpdate update, IDictionary<Uri, Uri> mapping)
        {
            if (update.Relations?.Added == null) return;
            foreach (var relation in update.Relations.Added)
            {
                if (!mapping.TryGetValue(relation.Url, out var url)) continue;
                
                relation.Url = url;
                document.Add("/relations/-", relation);
            }
        }
    }
}