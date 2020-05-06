using System;
using System.IO;
using System.Threading.Tasks;
using AzureDevOpsRest;
using AzureDevOpsRest.Requests;
using FluentAssertions;
using MigrateWorkItems.Data;
using MigrateWorkItems.Model;
using NSubstitute;
using Xunit;

namespace MigrateWorkItems.Tests
{
    public class AttachmentsProcessorTests : IClassFixture<TemporaryContext>
    {
        private readonly MigrationContext _context;
        private static readonly Guid OldId = new Guid("4410d451-8e90-4aeb-86ea-e501056a0706");

        public AttachmentsProcessorTests(TemporaryContext context) => _context = context.Context;

        [Fact]
        public async Task Test()
        {
            var config = new TestConfig();

            var newId = Guid.NewGuid();

            var client = Substitute.For<IClient>();
            client
                .PostAsync(Arg.Any<Request<AttachmentReference>>(), Arg.Any<Stream>())
                .Returns(new AttachmentReference { Id = newId, Url = new Uri($"https://some-url/{newId}") });
            
                _context.Attachments.Add(new AttachmentMapping
                {
                    Id = OldId,
                    FileName = "image.png"
                });
                await _context.SaveChangesAsync();

                var organization = config.Organization;
                var project = config.Project;

                await AttachmentsProcessor.UploadAttachments(client, organization, project, _context, "");

                await client
                    .Received()
                    .PostAsync(Arg.Any<Request<AttachmentReference>>(), Arg.Any<Stream>());

                _context
                    .Attachments
                    .Find(OldId)
                    .Url
                    .Should()
                    .Be($"https://some-url/{newId}");
        }

        [Fact]
        public async Task Skips()
        {
            var client = Substitute.For<IClient>();

                _context.Attachments.Add(new AttachmentMapping
                {
                    Id = OldId,
                    Url = new Uri("https://www.google.com")
                });
                await _context.SaveChangesAsync();

                await AttachmentsProcessor.UploadAttachments(client, "", "", _context, "");

                await client
                    .DidNotReceive()
                    .PostAsync(Arg.Any<Request<AttachmentReference>>(), Arg.Any<Stream>());
        }
    }
}