using System;
using System.Threading.Tasks;
using FluentAssertions;
using MigrateWorkItems.Model;
using Xunit;

namespace MigrateWorkItems.Tests
{
    public class MapperTests : IClassFixture<TemporaryContext>
    {
        private readonly MigrationContext _context;

        public MapperTests(TemporaryContext context) => _context = context.Context;

        [Fact]
        public void Attachment()
        {
            var id = Guid.NewGuid();
            _context.Attachments.Add(new AttachmentMapping
            {
                Id = id,
                Url = new Uri("https://www.google.com")
            });
            
            var mapper = new Mapper(_context);
            mapper
                .TryGetAttachment(id, out var to)
                .Should()
                .BeTrue();

            to.Should().Be(new Uri("https://www.google.com"));
        }
    }
}