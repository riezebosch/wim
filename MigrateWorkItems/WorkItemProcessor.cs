using System;
using System.Linq;
using System.Threading.Tasks;
using AzureDevOpsRest;
using AzureDevOpsRest.Data.WorkItems;
using AzureDevOpsRest.Requests;
using Microsoft.AspNetCore.JsonPatch;
using MigrateWorkItems.Data;
using MigrateWorkItems.FieldsProcessors;
using MigrateWorkItems.Relations;

namespace MigrateWorkItems
{
    public class WorkItemProcessor
    {
        private readonly IFieldsProcessor[] _processors;
        private static IRelationsProcessors _relations;
        private readonly IMapper _mapper;
        private readonly IClient _client;

        public WorkItemProcessor(string project,
            IClient client,
            IFieldsResolver resolver, 
            IRelationsProcessors relations,
            IMapper mapper)
        {
            _processors = new IFieldsProcessor[] {
                new NullProcessor(),
                new ReplaceAttachments(mapper), 
                new ClassificationNodes(project),
                new ClassificationNodesResetToTeamProject(project), 
                new RemoveAutoFields(),
                new RemoveNotFoundFields(resolver), 
                new ReadOnlyFields(),
                new RevisedToChangedFields()
            };

            _client = client;
            _relations = relations;
            _mapper = mapper;
        }

        private async Task UpdateWorkItem(WorkItemUpdate update)
        {
            var document = new JsonPatchDocument();
            await UpdateFields(document, update);
            await UpdateRelations(document, update);

            if (!_mapper.TryGetWorkItem(update.WorkItemId, out var uri))
            {
                throw new Exception($"Trying to update item {update.WorkItemId} with update {update.Id} but no mapping found.");
            }
            
            if (!document.Operations.All(x => x.path == "/fields/System.ChangedBy" || x.path == "/fields/System.ChangedDate"))
            {
                await _client.PatchAsync(
                    new UriRequest<WorkItem>(uri, "5.1")
                        .WithQueryParams(("bypassRules", true))
                        .WithHeaders(("Content-Type", "application/json-patch+json")), document);
            }
        }

        private static async Task UpdateRelations(JsonPatchDocument document, WorkItemUpdate update)
        {
            await _relations.Execute(document, update);
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

        public async Task Process(string organization, 
            string project,
            WorkItemUpdate update)
        {
            if (update.Id == 1)
            {
                await CreateWorkItem(organization, project, update);
            }
            else if (update.Fields != null && update.Fields.ContainsKey("System.IsDeleted"))
            {
                // skip delete and undelete work items for now.    
            }
            else
            {
                await UpdateWorkItem(update);
            }
        }

        private async Task CreateWorkItem(string organization, 
            string project,
            WorkItemUpdate update)
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

            await UpdateRelations(document, update);

            var type = update.Fields["System.WorkItemType"].NewValue;
            var item = await _client.PostAsync(
                new Request<WorkItem>($"{organization}/{project}/_apis/wit/workitems/${type}", "5.1")
                    .WithQueryParams(("bypassRules", true))
                    .WithHeaders(("Content-Type", "application/json-patch+json")), document);

            await _mapper.WorkItem(update.WorkItemId, item.Url);
        }
    }

    public interface IMapper
    {
        Task WorkItem(int from, Uri to);
        bool TryGetWorkItem(int id, out Uri url);
        bool TryGetAttachment(Guid from, out Uri to);
    }
}