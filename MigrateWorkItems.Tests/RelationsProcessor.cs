using System;
using System.Collections.Generic;
using AzureDevOpsRest;
using AzureDevOpsRest.Data.WorkItems;
using Microsoft.AspNetCore.JsonPatch;
using MigrateWorkItems.Tests.Data;

namespace MigrateWorkItems.Tests
{ 
    internal class RelationsProcessor
    {
        private  readonly IRelationsProcessor[] _processors;

        public RelationsProcessor(IClient client)
        {
            _processors = new IRelationsProcessor[] { new AddRelations(), new RemoveRelations(client) };
        }

        public void Execute(JsonPatchDocument document, Uri item, WorkItemUpdate update, IDictionary<Uri, Uri> mapping)
        {
            foreach (var processor in _processors)
            {
                processor.Execute(document, item, update, mapping);
            }
        }
    }
}