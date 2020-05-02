using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureDevOpsRest;
using AzureDevOpsRest.Requests;
using FluentAssertions;
using MigrateWorkItems.Tests.Data;
using MigrateWorkItems.Tests.FieldsProcessors;
using Xunit;
using Xunit.Abstractions;

namespace MigrateWorkItems.Tests
{
    public class RemoveReadOnlyFieldsTests
    {
        [Fact]
        public async Task ReadOnlyFieldsShouldBeRemoved()
        {
            var config = new TestConfig();
            var client = new Client(config.Token);
            
            var update = new WorkItemUpdate
            {
                Fields = await ReadOnlyFields(config, client)
            };
            
            var processor = new ReadOnlyFields();
            await processor.Execute(update);

            update
                .Fields
                .Keys
                .Should()
                .BeEmpty();
        }

        private static async Task<IDictionary<string, Value>> ReadOnlyFields(TestConfig config, IClient client)
        {
            var fields = new Dictionary<string, Value>();
            await foreach (var type in new Client(new TestConfig().Token).GetAsync(
                new Request<WorkItemType>($"{config.Organization}/{config.Project}/_apis/wit/workItemTypes/", "5.1").AsEnumerable()))
            {
                foreach (var field in type.Fields.Select(x => x.Url).Distinct())
                {
                    var data = await client.GetAsync(new UriRequest<WorkItemField>(field, "5.1"));
                    if (data != null && data.ReadOnly)
                    {
                        fields[data.ReferenceName] = new Value();
                    }
                }
            }

            return fields;
        }
    }
}