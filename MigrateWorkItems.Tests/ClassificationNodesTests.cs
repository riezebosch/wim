using System.Collections.Generic;
using FluentAssertions;
using MigrateWorkItems.Tests.Data;
using MigrateWorkItems.Tests.FieldsProcessors;
using Xunit;

namespace MigrateWorkItems.Tests
{
    public class ClassificationNodesTests
    {
        [Fact]
        public void Replace()
        {
            // Arrange
            var processor = new ClassificationNodes("migration-target");
            var update = new WorkItemUpdate
            {
                Fields = new Dictionary<string, Value>
                {
                    ["System.AreaPath"] = new Value { NewValue = "test" }
                }
            };
            
            // Act
            processor.Execute(update);
            
            // Assert
            update
                .Fields["System.AreaPath"]
                .NewValue
                .Should()
                .Be("migration-target");
        }
        
        [Fact]
        public void ReplaceOnlyFirstPart()
        {
            // Arrange
            var processor = new ClassificationNodes("migration-target");
            var update = new WorkItemUpdate
            {
                Fields = new Dictionary<string, Value>
                {
                    ["System.AreaPath"] = new Value { NewValue = @"test\area\path" }
                }
            };
            
            // Act
            processor.Execute(update);
            
            // Assert
            update
                .Fields["System.AreaPath"]
                .NewValue.Should()
                .Be(@"migration-target\area\path");
        }
        
        [Fact]
        public void IterationPath()
        {
            // Arrange
            var processor = new ClassificationNodes("migration-target");
            var update = new WorkItemUpdate
            {
                Fields = new Dictionary<string, Value>
                {
                    ["System.IterationPath"] = new Value { NewValue = @"test\Sprint 1" }
                }
            };
            
            // Act
            processor.Execute(update);
            
            // Assert
            update
                .Fields["System.IterationPath"]
                .NewValue.Should()
                .Be(@"migration-target\Sprint 1");
        }
    }
}