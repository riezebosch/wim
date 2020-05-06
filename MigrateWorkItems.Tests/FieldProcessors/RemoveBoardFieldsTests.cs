using System.Collections.Generic;
using FluentAssertions;
using MigrateWorkItems.Data;
using MigrateWorkItems.FieldsProcessors;
using Xunit;

namespace MigrateWorkItems.Tests.FieldProcessors
{
    public class RemoveAutoFieldsTests
    {
        [Theory]
        [InlineData("WEF_BE8875CA12F140109DFDBC966C5C5A9D_System.ExtensionMarker"),
        InlineData("WEF_BE8875CA12F140109DFDBC966C5C5A9D_Kanban.Column"),
        InlineData("WEF_BE8875CA12F140109DFDBC966C5C5A9D_Kanban.Column.Done")]
        public void RemoveFields(string field)
        {
            var processor = new RemoveAutoFields();
            var update = new WorkItemUpdate
            {
                Fields = new Dictionary<string, Value>
                {
                    [field] = new Value()
                }
            };
            
            processor.Execute(update);

            update.Fields.Should().BeEmpty();
        }
    }
}