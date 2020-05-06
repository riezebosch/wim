using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureDevOpsRest;
using AzureDevOpsRest.Data;
using AzureDevOpsRest.Data.WorkItems;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Xunit;

namespace MigrateWorkItems.Tests
{
    public class SaveWorkItemsTests
    {
        [Fact]
        public void To()
        {
            var config = new TestConfig();
            var client = new Client(config.Token);

            var output = Directory.CreateDirectory(config.Organization).CreateSubdirectory(config.Project);
            var items = output.CreateSubdirectory("items");
            var attachments = output.CreateSubdirectory("attachments");
            var save = new SaveWorkItems(client);

            save.To(items,
                    config.Organization,
                    attachments,
                    config.Project)
                .ToEnumerable()
                .Should()
                .NotBeEmpty();
        }
        
        [Fact]
        public void Attachment()
        {
            var client = Substitute.For<IClient>();
            client
                .PostAsync(Arg.Any<IRequest<WorkItemQueryResult>>(), Arg.Any<object>())
                .Returns(
                    new WorkItemQueryResult
                    {
                        WorkItems = new[]
                        {
                            new WorkItemRef()
                        }
                    }, new WorkItemQueryResult
                    {
                        WorkItems = new WorkItemRef[0]
                    });

            client
                .GetAsync(Arg.Any<IEnumerableRequest<JToken>>())
                .Returns(AsyncEnumerable.Repeat<JToken>(JToken.FromObject(new
                {
                    id = 1,
                    fields = new Dictionary<string, object>
                    {
                        ["System.Description"] = new 
                        {
                            newValue = "<div><img src=\"https://dev.azure.com/manuel/eb082604-a70f-4977-9335-85f0da463818/_apis/wit/attachments/f434ff22-1f72-4026-852d-a98470dba8b0?fileName=eenhoorn.png\" alt=eenhoorn.png><br></div>"
                        }
                    }
                }), 1));
            
            client
                .GetAsync(Arg.Any<IRequest<Stream>>())
                .Returns(File.OpenRead(Path.Join("items", "2195", "1.json")));

            var items = Directory.CreateDirectory("test").CreateSubdirectory(Guid.NewGuid().ToString());
            var attachments = Directory.CreateDirectory("test").CreateSubdirectory(Guid.NewGuid().ToString());
            
            var save = new SaveWorkItems(client);
            save
                .To(items, "", attachments, @"SME\Multivers", @"SME\Web based", @"SME\Administrative")
                .ToEnumerable()
                .Should()
                .NotBeEmpty();

            File
                .Exists(Path.Join(attachments.FullName, "f434ff22-1f72-4026-852d-a98470dba8b0"))
                .Should()
                .BeTrue();
        }
    }
}