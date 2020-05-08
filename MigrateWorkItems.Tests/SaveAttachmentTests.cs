using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureDevOpsRest;
using FluentAssertions;
using MigrateWorkItems.Save;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Xunit;

namespace MigrateWorkItems.Tests
{
    public class SaveAttachmentTests
    {
        [Fact]
        public void FromDescription()
        {
            var client = Substitute.For<IClient>();
            client
                .GetAsync(Arg.Any<IRequest<Stream>>())
                .Returns(File.OpenRead(Path.Join("items", "2195", "1.json")));

            Directory.CreateDirectory("test").CreateSubdirectory(Guid.NewGuid().ToString());
            var attachments = Directory.CreateDirectory("test").CreateSubdirectory(Guid.NewGuid().ToString());
            
            var save = new SaveAttachments(client, attachments);
            save.To(JToken.FromObject(new
                {
                    id = 1,
                    fields = new Dictionary<string, object>
                    {
                        ["System.Description"] = new
                        {
                            newValue =
                                "<div><img src=\"https://dev.azure.com/manuel/eb082604-a70f-4977-9335-85f0da463818/_apis/wit/attachments/f434ff22-1f72-4026-852d-a98470dba8b0?fileName=eenhoorn.png\" alt=eenhoorn.png><br></div>"
                        }
                    }
                })).ToEnumerable()
                .Should()
                .HaveCount(1);

            File
                .Exists(Path.Join(attachments.FullName, "f434ff22-1f72-4026-852d-a98470dba8b0"))
                .Should()
                .BeTrue();
        }
        
        [Fact]
        public void FromRelations()
        {
            var client = Substitute.For<IClient>();
            client
                .GetAsync(Arg.Any<IRequest<Stream>>())
                .Returns(File.OpenRead(Path.Join("items", "2195", "1.json")));

            Directory.CreateDirectory("test").CreateSubdirectory(Guid.NewGuid().ToString());
            var attachments = Directory.CreateDirectory("test").CreateSubdirectory(Guid.NewGuid().ToString());
            
            var save = new SaveAttachments(client, attachments);
            save.To(JToken.FromObject(new
                {
                    id = 1,
                    relations = new {
                        added = new[] 
                        {
                            new
                            {
                                rel = "AttachedFile",
                                url = "https://dev.azure.com/manuel/eb082604-a70f-4977-9335-85f0da463818/_apis/wit/attachments/078b6b63-3743-40e3-be66-38c6f0102a02"
                            },
                            new 
                            {
                                rel = "ArtifactLink",
                                url = "vstfs:///Git/Commit/eb082604-a70f-4977-9335-85f0da463818%2Fa5d81f51-d461-4cb2-baa7-e6fe52e193c2%2Ff4ef383ddb19d39815318df34bbcaf4ea88536e6",
                            }
                        }
                    }
                })).ToEnumerable()
                .Should()
                .HaveCount(1);

            File
                .Exists(Path.Join(attachments.FullName, "078b6b63-3743-40e3-be66-38c6f0102a02"))
                .Should()
                .BeTrue();
        }
    }
}