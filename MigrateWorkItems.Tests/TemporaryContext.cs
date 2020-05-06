using System;
using System.IO;
using System.Threading.Tasks;
using MigrateWorkItems.Model;
using Xunit;

namespace MigrateWorkItems.Tests
{
    public class TemporaryContext : IAsyncLifetime
    {
        private readonly DirectoryInfo _output;

        public TemporaryContext()
        {
            _output = new DirectoryInfo(Guid.NewGuid().ToString("N"));
            _output.Create();
            
            Context = new MigrationContext(_output.FullName);
        }

        public MigrationContext Context { get; }


        public Task InitializeAsync() => Context.Database.EnsureCreatedAsync();

        public async Task DisposeAsync()
        {
            await Context.Database.EnsureDeletedAsync();
            _output.Delete(true);
        }
    }
}