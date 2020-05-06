using System.IO;
using Microsoft.EntityFrameworkCore;

namespace MigrateWorkItems.Model
{
    public class MigrationContext : DbContext
    {
        private readonly string _db;

        public MigrationContext(string output) => _db = Path.Join(output, "migration.db");

        protected override void OnConfiguring(DbContextOptionsBuilder options) =>
            options.UseSqlite($"Data Source={_db}");

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Update>().HasKey(c => new { c.Id, c.WorkItemId });
        }

        public DbSet<Update> Updates { get; set; }
        public DbSet<WorkItemMapping> WorkItemMapping { get; set; }
    }
}