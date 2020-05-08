using System;
using System.Linq;
using AzureDevOpsRest.Data.WorkItems;
using FluentAssertions;
using Microsoft.AspNetCore.JsonPatch;
using MigrateWorkItems.Data;
using MigrateWorkItems.Relations;
using NSubstitute;
using Xunit;

namespace MigrateWorkItems.Tests.Relations
{
    public class AddAttachedFilesTests
    {
        [Fact]
        public void Add()
        {
            var document = new JsonPatchDocument();
            var item = new WorkItemUpdate
            {
                Relations = new Data.Relations
                {
                    Added = new[]
                    {
                        new Relation
                        {
                            Rel = "AttachedFile",
                            Url = new Uri("https://dev.azure.com/manuel/eb082604-a70f-4977-9335-85f0da463818/_apis/wit/attachments/1f70b0ee-13c9-4989-abd3-6668dc22cdf8")
                        }
                    }
                }
            };

            var mapper = Substitute.For<IMapper>();
            mapper
                .TryGetAttachment(new Guid("1f70b0ee-13c9-4989-abd3-6668dc22cdf8"), out Arg.Any<Uri>())
                .Returns(x =>
                {
                    x[1] = new Uri("https://www.google.com");
                    return true;
                });

            var processor = new AddAttachedFiles(mapper);
            processor.Execute(document, item);

            document
                .Operations
                .Should()
                .HaveCount(1);
        }
        
        [Fact]
        public void Empty()
        {
            var document = new JsonPatchDocument();
            var item = new WorkItemUpdate();

            var processor = new AddAttachedFiles(Substitute.For<IMapper>());
            processor.Execute(document, item);

            document
                .Operations
                .Should()
                .HaveCount(0);
        }
        
        [Fact]
        public void NothingAdded()
        {
            var document = new JsonPatchDocument();
            var item = new WorkItemUpdate
            {
                Relations = new Data.Relations()
            };

            var processor = new AddAttachedFiles(Substitute.For<IMapper>());
            processor.Execute(document, item);

            document
                .Operations
                .Should()
                .HaveCount(0);
        }
        
        [Fact]
        public void Other()
        {
            var document = new JsonPatchDocument();
            var item = new WorkItemUpdate
            {
                Relations = new Data.Relations
                {
                    Added = new[]
                    {
                        new Relation
                        {
                            Rel = "other",
                            Url = new Uri("https://www.google.com/")
                        }
                    }
                }
            };

            var processor = new AddAttachedFiles(Substitute.For<IMapper>());
            processor.Execute(document, item);

            document
                .Operations
                .Should()
                .HaveCount(0);
        }
    }
}