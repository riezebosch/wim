using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureDevOpsRest;
using AzureDevOpsRest.Data.WorkItems;
using AzureDevOpsRest.Requests;
using Microsoft.AspNetCore.JsonPatch;
using MigrateWorkItems.Tests.Data;
using MigrateWorkItems.Tests.FieldsProcessors;

namespace MigrateWorkItems.Tests
{
    public class WorkItemProcessor
    {
        private readonly IFieldsProcessor[] _processors;

        public WorkItemProcessor(string project)
        {
            _processors = new IFieldsProcessor[] {
                new ClassificationNodes(project),
                new ReadOnlyFields()
            };
        }
        
        public async Task UpdateWorkItem(Client client, WorkItem item, IEnumerable<WorkItemUpdate> updates)
        {
            foreach (var update in updates)
            {
                await UpdateWorkItem(client, item.Url, update);
            }
        }

        private async Task UpdateWorkItem(Client client, Uri item, WorkItemUpdate update)
        {
            var document = new JsonPatchDocument();
            UpdateFields(document, update);

            if (document.Operations.Any())
            {
                await client.PatchAsync(
                    new UriRequest<WorkItem>(item, "5.1")
                        .WithQueryParams(("bypassRules", true))
                        .WithHeaders(("Content-Type", "application/json-patch+json")), document);
            }
        }

        private void UpdateFields(JsonPatchDocument document, WorkItemUpdate update)
        {
            if (update.Fields == null) return;

            foreach (var processor in _processors)
            {
                processor.Execute(update);
            }
            
            foreach (var (key, value) in update.Fields)
            {
                document.Replace($"fields/{key}", value.NewValue);
            }
        }

        public async Task<WorkItem> CreateWorkItem(Client client, string organization, string project, WorkItemUpdate update)
        {
            // May not be null but is in the updates work item updates API.
            update.Fields["System.AssignedTo"].NewValue ??= "";
            foreach (var processor in _processors)
            {
                processor.Execute(update);
            }
            
            var document = new JsonPatchDocument();
            foreach (var (key, value) in update.Fields)
            {
                document.Add($"fields/{key}", value.NewValue);
            }

            var type = update.Fields["System.WorkItemType"].NewValue;
            var item = await client.PostAsync(
                new Request<WorkItem>($"{organization}/{project}/_apis/wit/workitems/${type}", "5.1")
                    .WithQueryParams(("bypassRules", true))
                    .WithHeaders(("Content-Type", "application/json-patch+json")), document);
            
            return item;
        }
    }
}