using System;
using System.Collections.Generic;
using FluentAssertions;
using MigrateWorkItems.Tests.Data;
using MigrateWorkItems.Tests.FieldsProcessors;
using Newtonsoft.Json.Linq;
using Xunit;

namespace MigrateWorkItems.Tests
{
    public class RevisedToChangedFieldsTests
    {
        [Fact]
        public void IfEmpty()
        {
            var me = JToken.FromObject(new {Name = "me"});
            var when = new DateTime(2012, 2, 12);
            
            var update = new WorkItemUpdate
            {
                Id = 2,
                RevisedBy = me,
                RevisedDate = when
            };

            new RevisedToChangedFields().Execute(update);

            update
                .Fields["System.ChangedBy"]
                .NewValue
                .Should()
                .Be(me);

            update
                .Fields["System.ChangedDate"]
                .NewValue
                .Should()
                .Be(when);
        }
        
        [Fact]
        public void IfNotEmpty()
        {
            var me = JToken.FromObject(new {Name = "me"});
            var update = new WorkItemUpdate
            {
                Id = 2,
                RevisedBy = me,
                Fields = new Dictionary<string, Value>
                {
                    ["System.ChangedBy"] = new Value { NewValue = "someone" }
                }
            };

            new RevisedToChangedFields().Execute(update);

            update
                .Fields["System.ChangedBy"]
                .NewValue
                .Should()
                .Be("someone");
        }
        
        [Fact]
        public void IfCreated()
        {
            var me = JToken.FromObject(new {Name = "me"});
            var when = new DateTime(9999, 1, 1);
            
            var update = new WorkItemUpdate
            {
                Id = 1,
                RevisedBy = me,
                RevisedDate = when
            };

            new RevisedToChangedFields().Execute(update);

            update
                .Fields
                .Should()
                .BeNull();
        }
    }
}