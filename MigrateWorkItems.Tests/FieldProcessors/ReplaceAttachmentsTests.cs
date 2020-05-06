using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using MigrateWorkItems.Data;
using MigrateWorkItems.FieldsProcessors;
using NSubstitute;
using Xunit;

namespace MigrateWorkItems.Tests.FieldProcessors
{
    public class ReplaceAttachmentsTests
    {
        [Fact]
        public async Task Test()
        {
            var uri = new Uri($"https://new-uri/{Guid.NewGuid()}");
            var mapper = Substitute.For<IMapper>();
            mapper
                .TryGetAttachment(Arg.Any<Guid>(), out Arg.Any<Uri>())
                .Returns(x =>
                {
                    x[1] = uri;
                    return true;
                });
            var item = new WorkItemUpdate
            {
                Fields = new Dictionary<string, Value>
                {
                    ["System.Description"] = new Value
                    {
                        NewValue = "https://dev.azure.com/new/00000000-a70f-4977-9335-85f0da463818/_apis/wit/attachments/f434ff22-0000-4026-852d-a98470dba8b0"
                    }
                }
            };

            var processor = new ReplaceAttachments(mapper);
            await processor.Execute(item);
                
            item
                .Fields["System.Description"]
                .NewValue
                .Should()
                .Be(uri.ToString());
        }
        
        [Fact]
        public async Task NoDescription()
        {
            var item = new WorkItemUpdate
            {
                Fields = new Dictionary<string, Value>()
            };


            var processor = new ReplaceAttachments(Substitute.For<IMapper>());
            await processor.Execute(item);

            item
                .Fields
                .Should()
                .BeEmpty();
        }
        
        [Fact]
        public async Task NoFields()
        {
            var item = new WorkItemUpdate();

            var processor = new ReplaceAttachments(Substitute.For<IMapper>());
            await processor.Execute(item);
        }
    }
}