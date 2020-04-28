using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureDevOpsRest;
using AzureDevOpsRest.Data.WorkItems;
using AzureDevOpsRest.Requests;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;
using Xunit.Abstractions;

namespace MigrateWorkItems.Tests
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper _output;

        public UnitTest1(ITestOutputHelper output) => _output = output;

        [Fact]
        public async Task Test1()
        {
            var config = new TestConfig();
            var client = new AzureDevOpsRest.Client(config.Organization, config.Token);

            var manuel = new
            {
                displayName = "M. Riezebosch",
                url =
                    "https://spsprodeus22.vssps.visualstudio.com/Ac00f72f0-b331-4ca5-8a23-19a7bd2f3c69/_apis/Identities/61111e88-4ba0-4a4a-98d4-474232ffb10b",
                _links = new
                {
                    avatar = new
                    {
                        href =
                            "https://dev.azure.com/manuel/_apis/GraphProfile/MemberAvatars/msa.ZTlkMTNhM2QtN2JjMC03YWY4LWIxNDItMjIwZjA2Yzc3Mjky"
                    }
                },
                id = "61111e88-4ba0-4a4a-98d4-474232ffb10b",
                uniqueName = "mriezebosch@gmail.com",
                imageUrl =
                    "https://dev.azure.com/manuel/_apis/GraphProfile/MemberAvatars/msa.ZTlkMTNhM2QtN2JjMC03YWY4LWIxNDItMjIwZjA2Yzc3Mjky",
                descriptor = "msa.ZTlkMTNhM2QtN2JjMC03YWY4LWIxNDItMjIwZjA2Yzc3Mjky"
            };
            
            var document = new JsonPatchDocument()
                .Add("fields/System.NodeName", "test")
                .Add("fields/System.AreaLevel1", "test")
                .Add("fields/System.AuthorizedDate", "2020-04-24T19:04:41.977Z")
                .Add("fields/System.RevisedDate", "2020-04-24T19:05:44.873Z")
                .Add("fields/System.IterationLevel1", "test")
                .Add("fields/System.IterationLevel2", "Sprint 1")
                .Add("fields/System.WorkItemType", "Epic")
                .Add("fields/System.State", "New")
                .Add("fields/System.Reason", "New epic")
                .Add("fields/System.AssignedTo", "")
                .Add("fields/System.CreatedDate", "2020-04-24T19:04:41.977Z")
                .Add("fields/System.CreatedBy", manuel)
                .Add("fields/System.ChangedDate", "2020-04-24T19:04:41.977Z")
                .Add("fields/System.ChangedBy", manuel)
                .Add("fields/System.AuthorizedAs", manuel)
                .Add("fields/System.CommentCount", 0)
                .Add("fields/System.TeamProject", "test")
                .Add("fields/System.AreaPath", "test")
                .Add("fields/System.IsDeleted", false)
                .Add("fields/System.IterationPath", "test\\Sprint 1")
                .Add("fields/System.Title", "start as epic")
                .Add("fields/Microsoft.VSTS.Common.StateChangeDate", "2020-04-24T19:04:41.977Z")
                .Add("fields/Microsoft.VSTS.Common.Priority", 2)
                .Add("fields/Microsoft.VSTS.Common.ValueArea", "Business")
                ;

            var data = System.Text.Json.JsonSerializer.Serialize(document);
            _output.WriteLine(data);
            
                var item = await client.PostAsync(
                    new Request<WorkItem>("test/_apis/wit/workitems/$Epic", "5.1").WithQueryParams(("bypassRules", true)).WithHeaders(("Content-Type", "application/json-patch+json")), document);
                
                // await client.PatchAsync(
                //     new Request<WorkItem>($"test/_apis/wit/workitems/{item.Id}", "5.1").WithHeaders(("Content-Type", "application/json-patch+json")), document);

        }
        
        [Fact]
        public async Task TestFromJson()
        {
            const string project = "test";
            var config = new TestConfig();
            var client = new Client(config.Organization, config.Token);

            var json = File.ReadAllText(Path.Combine("items", "wi-updates.json"));
            var updates = JsonConvert.DeserializeObject<WorkItemUpdates>(json, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            var id = await CreateWorkItem(client, project, updates.Value.First());
            await UpdateWorkItem(client, project, id, updates.Value.Skip(1));
        }

        private static async Task UpdateWorkItem(Client client, string project, int id,
            IEnumerable<WorkItemUpdate> updates)
        {
            var document = new JsonPatchDocument();
            foreach (var update in updates)
            {
                RemoveReadOnlyFields(update, "System.BoardColumn", "System.BoardColumnDone");
                foreach (var (key, value) in update.Fields)
                {
                    document.Replace($"fields/{key}", value.NewValue);
                }

                await client.PatchAsync(
                    new Request<WorkItem>($"{project}/_apis/wit/workitems/{id}", "5.1")
                        .WithQueryParams(("bypassRules", true))
                        .WithHeaders(("Content-Type",
                        "application/json-patch+json")), document);
            }
        }

        private static void RemoveReadOnlyFields(WorkItemUpdate update, params string[] fields)
        {
            foreach (var field in fields)
            {
                RemoveReadOnlyField(update, field);
            }
        }

        private static void RemoveReadOnlyField(WorkItemUpdate update, string field) => update.Fields.Remove(field);

        private static async Task<int> CreateWorkItem(Client client, string project, WorkItemUpdate first)
        {
            // May not be null but is in the updates work item updates API.
            first.Fields["System.AssignedTo"].NewValue ??= "";
            
            var document = new JsonPatchDocument();
            foreach (var field in first.Fields)
            {
                document.Add($"fields/{field.Key}", field.Value.NewValue);
            }

            var type = (string)first.Fields["System.WorkItemType"].NewValue;
            var item = await client.PostAsync(
                new Request<WorkItem>($"{project}/_apis/wit/workitems/${type}", "5.1")
                    .WithQueryParams(("bypassRules", true))
                    .WithHeaders(("Content-Type", "application/json-patch+json")), document);
            return item.Id;
        }
    }

    public class WorkItemUpdates
    {
        public IEnumerable<WorkItemUpdate> Value { get; set; }
    }

    public class WorkItemUpdate
    {
        public IDictionary<string, Value> Fields { get; set; }
    }

    public class Value
    {
        public object NewValue { get; set; }
    }
}