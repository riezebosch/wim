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

        public WorkItemProcessor(string project, IFieldsResolver resolver)
        {
            _processors = new IFieldsProcessor[] {
                new NullProcessor(),
                new ClassificationNodes(project),
                new ClassificationNodesResetToTeamProject(project), 
                new RemoveAutoFields(),
                new RemoveNotFoundFields(resolver), 
                new ReadOnlyFields(),
                new RevisedToChangedFields()
            };
        }

        private async Task UpdateWorkItem(Client client, 
            Uri item,
            WorkItemUpdate update,
            IDictionary<Uri, Uri> mapping)
        {
            var document = new JsonPatchDocument();
            await UpdateFields(document, update);
            await UpdateRelations(client, item, document, update, mapping);

            if (!document.Operations.All(x => x.path == "/fields/System.ChangedBy" || x.path == "/fields/System.ChangedDate"))
            {
                await client.PatchAsync(
                    new UriRequest<WorkItem>(item, "5.1")
                        .WithQueryParams(("bypassRules", true))
                        .WithHeaders(("Content-Type", "application/json-patch+json")), document);
            }
        }

        private static async Task UpdateRelations(IClient client, Uri uri, JsonPatchDocument document, WorkItemUpdate update,
            IDictionary<Uri, Uri> mapping)
        {
            await new RelationsProcessor(client).Execute(document, uri, update, mapping);
        }

        private async Task UpdateFields(JsonPatchDocument document, WorkItemUpdate update)
        {
            foreach (var processor in _processors)
            {
                await processor.Execute(update);
            }
            
            foreach (var (key, value) in update.Fields)
            {
                document.Replace($"fields/{key}", value.NewValue);
            }
        }

        public async Task Process(Client client, 
            string organization, 
            string project,
            Uri uri,
            WorkItemUpdate update, Dictionary<Uri, Uri> mapping)
        {
            if (update.Id == 1)
            {
                await CreateWorkItem(client, organization, project, uri, update, mapping);
            }
            else if (update.Fields != null && update.Fields.ContainsKey("System.IsDeleted"))
            {
                // skip delete and undelete work items for now.    
            }
            else
            {
                await UpdateWorkItem(client, mapping[uri], update, mapping);
            }
        }

        private async Task CreateWorkItem(Client client, string organization, string project, Uri uri, WorkItemUpdate update,
            IDictionary<Uri, Uri> mapping)
        {
            // May not be null but is in the updates work item updates API.
            update.Fields["System.AssignedTo"].NewValue ??= "";
            foreach (var processor in _processors)
            {
                await processor.Execute(update);
            }

            var document = new JsonPatchDocument();
            foreach (var (key, value) in update.Fields)
            {
                document.Add($"fields/{key}", value.NewValue);
            }

            await UpdateRelations(client, null, document, update, mapping);

            var type = update.Fields["System.WorkItemType"].NewValue;
            var item = await client.PostAsync(
                new Request<WorkItem>($"{organization}/{project}/_apis/wit/workitems/${type}", "5.1")
                    .WithQueryParams(("bypassRules", true))
                    .WithHeaders(("Content-Type", "application/json-patch+json")), document);

            mapping[uri] = item.Url;
        }
    }
}