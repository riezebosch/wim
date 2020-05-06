using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MigrateWorkItems.Index;
using MigrateWorkItems.Model;
using Xunit;

namespace MigrateWorkItems.Tests.Index
{
    public class AttachmentIndexerTests : IClassFixture<TemporaryContext>
    {
        private readonly MigrationContext _context;

        public AttachmentIndexerTests(TemporaryContext context) => _context = context.Context;

        [Fact]
        public async Task Attachments()
        {
            var attachments = new DirectoryInfo("attachments");
            await AttachmentIndexer.Index(_context, attachments);
            
            _context
                .Attachments
                .Any(x => x.Id == new Guid("4410d451-8e90-4aeb-86ea-e501056a0706"))
                .Should()
                .BeTrue();
        }
        
        [Fact]
        public async Task Update()
        {
            var attachments = new DirectoryInfo("attachments");
            await AttachmentIndexer.Index(_context, attachments);
            await AttachmentIndexer.Index(_context, attachments);
        }
    }
}