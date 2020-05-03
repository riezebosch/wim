using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
// using System.Linq;
using System.Threading.Tasks;
using AzureDevOpsRest;
using FluentAssertions;
using Flurl.Http;
using LiteDB;
using Microsoft.EntityFrameworkCore;
using MigrateWorkItems.Tests.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;
using Xunit.Abstractions;

namespace MigrateWorkItems.Tests
{
    public class WorkItemProcessorTests
    {
        private readonly ITestOutputHelper _output;

        public WorkItemProcessorTests(ITestOutputHelper output) => _output = output;

        [Fact]
        public async Task TestFromJson()
        {
            const string project = "migration-target";
            var config = new TestConfig();
            var client = new Client(config.Token);

            var processor = new WorkItemProcessor(project, new FieldsResolver(client, config.Organization, project));
            var mapping = new Dictionary<Uri, Uri>();

            // using var db = new LiteDatabase("Filename=sme.db;Mode=ReadOnly");
            // var col = db.GetCollection<Update>();
            
            await using var context = new MigrationContext();
            foreach (var item in context
                .Updates
                .AsQueryable()
                // .Where(x => !x.Done)
                .OrderBy(x => x.ChangeDate))
            {
                var update = FromFile(Path.Join("unit4", "SME", item.WorkItemId.ToString(), item.Id + ".json"));
                
                var uri = new Uri($"https://dev.azure.com/manuel/eb082604-a70f-4977-9335-85f0da463818/_apis/wit/workItems/{update.WorkItemId}");
                try
                {
                    await processor.Process(client, config.Organization, project, uri, update, mapping);
                    item.Done = true;
                    
                    await context.SaveChangesAsync();
                }
                catch (FlurlHttpException ex)
                {
                    _output.WriteLine(ex.Call.Request.RequestUri.ToString());
                    _output.WriteLine(ex.Call.RequestBody);

                    if (ex.Call.Response?.Content != null)
                    {
                        _output.WriteLine(await ex.Call.Response.Content.ReadAsStringAsync());
                    }

                    throw;
                }
            }

            // var result = await client.GetAsync(WorkItems.WorkItem(config.Organization, pbi1.Id, 
            //     "System.AreaPath",
            //     "System.TeamProject", 
            //     "System.IterationPath", 
            //     "System.WorkItemType"));
            // result.Fields["System.AreaPath"].Should().Be(project);
            // result.Fields["System.TeamProject"].Should().Be(project);
            // result.Fields["System.IterationPath"].Should().Be(@$"{project}\Sprint 1");
            // result.Fields["System.WorkItemType"].Should().Be("Product Backlog Item");
            //
            // var child = await client.GetAsync(WorkItems.WorkItem(config.Organization, task.Id, 
            //     "System.Parent"));
            // // child.Fields["System.Parent"].Should().Be(pbi2.Id);
        }

        private static IEnumerable<(Uri, WorkItemUpdate update)> LoadUpdates(string id, params int[] updates) =>
            updates
                .Select(x => Path.Join("items", id, $"{x}.json"))
                .Select(FromFile)
                .Select(x => (new Uri($"https://dev.azure.com/manuel/eb082604-a70f-4977-9335-85f0da463818/_apis/wit/workItems/{id}"), x));

        private static WorkItemUpdate FromFile(string file) 
            => Deserialize(File.ReadAllText(file));

        private static WorkItemUpdate Deserialize(string content) => 
            JsonConvert.DeserializeObject<WorkItemUpdate>(content, new JsonSerializerSettings {ContractResolver = new CamelCasePropertyNamesContractResolver()});

        [Fact]
        public void LoadOrder()
        {
            LoadUpdates("2195", 2, 3, 4).OrderBy(x => x.update.RevisedDate.Year != 9999 ? x.update.RevisedDate : x.update.Fields["System.ChangedDate"].NewValue)
                .Select(x => x.update.Id)
                .Should()
                .BeEquivalentTo(new[] { 2, 3, 4 }, options => options.WithStrictOrdering());
            
            LoadUpdates("2196", 2, 3).OrderBy(x => x.update.RevisedDate)
                .Select(x => x.update.Id)
                .Should()
                .BeEquivalentTo(new[] { 2, 3 }, options => options.WithStrictOrdering());
            
            LoadUpdates("2197", 2, 3, 4, 5).OrderBy(x => x.update.RevisedDate)
                .Select(x => x.update.Id)
                .Should()
                .BeEquivalentTo(new[] { 2, 3, 4, 5 }, options => options.WithStrictOrdering());
        }
        
        [Fact]
        public void Convert()
        {
            using var db = new LiteDatabase("sme.db");
            var col = db.GetCollection<Update>();
            col.EnsureIndex(x => x.Id);

            var updates =
                new DirectoryInfo(Path.Join("unit4", "SME"))
                    .EnumerateDirectories()
                    .SelectMany(x => x.EnumerateFiles());
            
            foreach (var file in updates.AsParallel())
            {
                var update = FromFile(file.FullName);
                col.Insert(new Update
                {
                    Id = new UpdateId
                    {
                        WorkItemId = update.WorkItemId,
                        Id = update.Id,
                    },
                    ChangeDate = (DateTime?) update.Fields?["System.ChangedDate"].NewValue 
                                 ?? (DateTime?) update.Fields?["System.CreatedDate"].NewValue 
                                 ?? update.RevisedDate,
                    Content = File.ReadAllText(file.FullName)
                });
            }
           
        }
        
        [Fact]
        public async Task ConvertToSqlite()
        {
            await using var context = new MigrationContext();
            await context.Database.EnsureCreatedAsync();
                
            var updates =
                new DirectoryInfo(Path.Join("unit4", "SME"))
                    .EnumerateDirectories()
                    .SelectMany(x => x.EnumerateFiles());

            int i = 0;
            foreach (var file in updates.AsParallel())
            {
                var update = FromFile(file.FullName);
                await context.Updates.AddAsync(new Update2
                {
                    WorkItemId = update.WorkItemId,
                    Id = update.Id,
                    ChangeDate = (DateTime?) update.Fields?["System.ChangedDate"].NewValue 
                                 ?? (DateTime?) update.Fields?["System.CreatedDate"].NewValue 
                                 ?? update.RevisedDate,
                    // Content = File.ReadAllText(file.FullName)
                });

                if (i++ % 1000 == 0)
                {
                    await context.SaveChangesAsync();
                }
            }
            
            await context.SaveChangesAsync();
        }

        // [Fact]
        // public async Task CreateNodes()
        // {
        //     const string project = "migration-target";
        //     var config = new TestConfig();
        //     var client = new Client(config.Token);
        //
        //     using var db = new LiteDatabase("sme.db");
        //     var col = db.GetCollection<Update>();
        //
        //     var areas = new List<string>();
        //     var iterations = new List<string>();
        //     foreach (var update in col.FindAll())
        //     {
        //         if (update.AreaPath != null)
        //         {
        //             areas.Add(update.AreaPath);
        //         }
        //
        //         
        //         if (update.IterationPath != null)
        //         {
        //             iterations.Add(update.IterationPath);
        //         }
        //     }
        //
        //     var exists =
        //         await client.GetAsync(
        //             new Request<Areas>($"{config.Organization}/{project}/_apis/wit/classificationnodes/areas", "5.1").WithQueryParams(("$depth", 1234)));
        //
        //     foreach (var area in areas)
        //     {
        //         var path = "";
        //         foreach (var name in area.Split('\\'))
        //         {
        //             try
        //             {
        //                 await client.PostAsync(
        //                     new Request<object>(
        //                         $"{config.Organization}/{config.Project}/_apis/wit/classificationnodes/areas/{path}",
        //                         "5.1"), new {Name = name});
        //             }
        //             catch (FlurlHttpException e) when (e.Call.HttpStatus == HttpStatusCode.Conflict)
        //             {
        //             }
        //
        //             path += name + "/";
        //         }
        //     }
        //
        //     // foreach (var iteration in iterations)
        //     // {
        //     //         await client.PostAsync(
        //     //             new Request<object>($"{config.Organization}/{project}/_apis/wit/classificationnodes/iterations",
        //     //                 "5.1"), new {Name = iteration});
        //     // }
        // }
    }

    public class MigrationContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options) =>
            options.UseSqlite("Data Source=migration.db");

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Update2>().HasKey(c => new { c.Id, c.WorkItemId });
        }

        public DbSet<Update2> Updates { get; set; }
    }

    public class Areas
    {
    }

    public class Update
    {
        // public int Id { get; set; }
        // public int WorkItemId { get; set; }
        public DateTime ChangeDate { get; set; }
        public UpdateId Id { get; set; }
        public string Content { get; set; }
        public bool Done { get; set; }
    }
    
    public class Update2
    {
        public int Id { get; set; }
        public int WorkItemId { get; set; }
        public DateTime ChangeDate { get; set; }
        // public UpdateId Id { get; set; }
        // public string Content { get; set; }
        public bool Done { get; set; }
    }

    public class UpdateId
    {
        public int Id { get; set; }
        public int WorkItemId { get; set; }
    }
}