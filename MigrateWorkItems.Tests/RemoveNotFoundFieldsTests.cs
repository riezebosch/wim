using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureDevOpsRest;
using FluentAssertions;
using MigrateWorkItems.Tests.Data;
using MigrateWorkItems.Tests.FieldsProcessors;
using NSubstitute;
using Xunit;

namespace MigrateWorkItems.Tests
{
    public class RemoveNotFoundFieldsTests
    {
        [Fact]
        public async Task NotFound()
        {
            var resolver = Substitute.For<IFieldsResolver>();
            resolver.ListAllFields().Returns(Enumerable.Empty<WorkItemField>());
                
            var processor = new RemoveNotFoundFields(resolver);
            
            var update = new WorkItemUpdate
            {
                Fields = new Dictionary<string, Value>
                {
                    ["SomeField"] = new Value()
                }
            };
            
            await processor.Execute(update);

            update
                .Fields
                .Keys
                .Should()
                .BeEmpty();
        }
    }
}