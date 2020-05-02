using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using MigrateWorkItems.Tests.Data;

namespace MigrateWorkItems.Tests
{
    internal class AddRelations : IRelationsProcessor
    {
        public Task Execute(JsonPatchDocument document, Uri original, WorkItemUpdate update,
            IDictionary<Uri, Uri> mapping)
        {
            if (update.Relations?.Added == null) return Task.CompletedTask;
            foreach (var relation in update.Relations.Added)
            {
                if (!mapping.TryGetValue(relation.Url, out var url)) continue;
                
                relation.Url = url;
                document.Add("/relations/-", relation);
            }
            
            return Task.CompletedTask;
        }
    }
}