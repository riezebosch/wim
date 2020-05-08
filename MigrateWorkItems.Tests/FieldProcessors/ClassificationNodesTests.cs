using System.Collections.Generic;
using FluentAssertions;
using MigrateWorkItems.Data;
using MigrateWorkItems.FieldsProcessors;
using NSubstitute;
using Xunit;

namespace MigrateWorkItems.Tests.FieldProcessors
{
    public class ClassificationNodesTests
    {
        [Fact]
        public void Replace()
        {
            // Arrange
            var processor = new ClassificationNodes("migration-target", Substitute.For<ICreateClassificationNodes>());
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
            var processor = new ClassificationNodes("migration-target", Substitute.For<ICreateClassificationNodes>());
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
            var processor = new ClassificationNodes("migration-target", Substitute.For<ICreateClassificationNodes>());
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
        
        [Fact]
        public void TriesToCreate()
        {
            // Arrange
            var nodes = Substitute.For<ICreateClassificationNodes>();
            var processor = new ClassificationNodes("migration-target", nodes);
            var update = new WorkItemUpdate
            {
                Fields = new Dictionary<string, Value>
                {
                    ["System.IterationPath"] = new Value { NewValue = @"test\Sprint 1" },
                    ["System.AreaPath"] = new Value { NewValue = @"test\my-project" }
                }
            };
            
            // Act
            processor.Execute(update);
            
            // Assert
            nodes
                .Received(1)
                .IfNotExists("iterations", "Sprint 1");
            
            nodes
                .Received(1)
                .IfNotExists("areas", "my-project");
        }
    }
}