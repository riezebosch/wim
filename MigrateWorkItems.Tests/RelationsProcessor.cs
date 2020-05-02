using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureDevOpsRest;
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

        public async Task Execute(JsonPatchDocument document, Uri item, WorkItemUpdate update, IDictionary<Uri, Uri> mapping)
        {
            foreach (var processor in _processors)
            {
                await processor.Execute(document, item, update, mapping);
            }
        }
    }
}