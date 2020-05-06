using System.Collections.Generic;
using System.Threading.Tasks;
using AzureDevOpsRest;
using AzureDevOpsRest.Requests;
using MigrateWorkItems.Data;

namespace MigrateWorkItems
{
    public class FieldsResolver : IFieldsResolver
    {
        private readonly IClient _client;
        private readonly string _organization;
        private readonly string _project;
        private IEnumerable<WorkItemField> _fields;

        public FieldsResolver(IClient client, string organization, string project)
        {
            _client = client;
            _organization = organization;
            _project = project;
        }
        private async Task<IEnumerable<WorkItemField>> LoadFields()
        {
            var fields = new List<WorkItemField>();
            await foreach (var type in _client.GetAsync(
                new Request<WorkItemType>($"{_organization}/{_project}/_apis/wit/workItemTypes/", "5.1").AsEnumerable()))
            {
                fields.AddRange(type.Fields);
            }

            return fields;
        }

        public async Task<IEnumerable<WorkItemField>> ListAllFields() => _fields ??= await LoadFields();
    }
}