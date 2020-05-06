using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MigrateWorkItems.Index;
using MigrateWorkItems.Model;
using Xunit;

namespace MigrateWorkItems.Tests.Index
{
    public class WorkItemIndexerTests
    {
        [Fact]
        public async Task Index()
        {
            const string output = "items";
            
            await using var context = new MigrationContext(output);
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            await WorkItemIndexer.Index(context, new DirectoryInfo(output));
            context
                .Updates
                .Any(x => x.Id == 1 && x.WorkItemId == 2195)
                .Should()
                .BeTrue();
        }
        
        [Fact]
        public async Task Update()
        {
            const string output = "items";
            
            await using var context = new MigrationContext(output);
            await context.Database.EnsureCreatedAsync();

            await WorkItemIndexer.Index(context, new DirectoryInfo(output));
            await WorkItemIndexer.Index(context, new DirectoryInfo(output));
        }
    }
}