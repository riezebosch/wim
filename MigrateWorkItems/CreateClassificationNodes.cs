using System.Collections.Generic;
using System.Threading.Tasks;
using AzureDevOpsRest;
using AzureDevOpsRest.Requests;

namespace MigrateWorkItems
{
    public interface ICreateClassificationNodes
    {
        Task IfNotExists(string type, params string[] areas);
    }

    public class CreateClassificationNodes : ICreateClassificationNodes
    {
        private readonly IClient _client;
        private readonly string _organization;
        private readonly string _project;
        private readonly HashSet<(string, string)> _cache = new HashSet<(string, string)>();

        public CreateClassificationNodes(IClient client, string organization, string project)
        {
            _client = client;
            _organization = organization;
            _project = project;
        }

        public async Task IfNotExists(string type, params string[] areas)
        {
            var parent = "";
            foreach (var area in areas)
            {
                parent = await Create(type, parent, area);
            }
        }

        private async Task<string> Create(string type, string parent, string child)
        {
            var path = $"{parent}/{child}";
            if (_cache.Add((type, path)))
            {
                var exists = new Request<object>($"{_organization}/{_project}/_apis/wit/classificationnodes/{type}/{path}", "5.1");
                if (await _client.GetAsync(exists) == null)
                {
                    var create = new Request<object>($"{_organization}/{_project}/_apis/wit/classificationnodes/{type}/{parent}", "5.1");
                    await _client.PostAsync(create, new {Name = child});
                }
            }

            return path;
        }
    }
}