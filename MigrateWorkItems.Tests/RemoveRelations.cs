using System;
using System.Collections.Generic;
using AzureDevOpsRest.Data.WorkItems;
using Microsoft.AspNetCore.JsonPatch;
using MigrateWorkItems.Tests.Data;

namespace MigrateWorkItems.Tests
{
    internal class RemoveRelations : IRelationsProcessor
    {
        public void Execute(JsonPatchDocument document, WorkItem item, WorkItemUpdate update, IDictionary<Uri, Uri> mapping)
        {
            if (update.Relations?.Removed == null) return;
            if (item.Relations == null) return;
            
            foreach (var relation in update.Relations.Removed)
            {
                if (!mapping.TryGetValue(relation.Url, out var url)) continue;
                
                var index = Array.FindIndex(item.Relations, x => x.Url == url && x.Rel == relation.Rel);
                if (index >= 0)
                {
                    document.Remove($"/relations/{index}");
                }
            }
        }
    }
}