using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureDevOpsRest;
using Flurl.Http;
using MigrateWorkItems.Data;
using MigrateWorkItems.Model;
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
            const string project = "SME";
            var config = new TestConfig();
            var client = new Client(config.Token);

            await using var context = new MigrationContext(Path.Join("unit4", "SME"));
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            
            var mapper = new Mapper(context);

            var processor = new WorkItemProcessor(
                project,
                client,
                new FieldsResolver(client, config.Organization, project), 
                new RelationsProcessors(client, mapper),
                mapper);

            var i = 0;
            try
            {
                foreach (var item in context
                    .Updates
                    .AsQueryable()
                    .Where(x => !x.Done)
                    .OrderBy(x => x.ChangeDate)
                    .ThenByDescending(x => x.Relations))
                {
                    var update = Indexer.FromFile(Path.Join("unit4", "SME", "items", item.WorkItemId.ToString(), item.Id + ".json"));

                    try
                    {
                        await processor.Process(config.Organization, project, update);
                        item.Done = true;
                        context.Updates.Update(item);
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

                    if (i++ > 1000)
                    {
                        await context.SaveChangesAsync();
                    }
                }
            }
            finally
            {
                await context.SaveChangesAsync();
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

        [Fact]
        public async Task Index()
        {
            var output = Path.Join("unit4", "SME");
            
            await using var context = new MigrationContext(output);
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            
            await context.Database.EnsureCreatedAsync();

            await Indexer.Index(context, new DirectoryInfo(output));
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

    public class Update
    {
        // public int Id { get; set; }
        // public int WorkItemId { get; set; }
        public DateTime ChangeDate { get; set; }
        public UpdateId Id { get; set; }
        public string Content { get; set; }
        public bool Done { get; set; }
    }

    public class UpdateId
    {
        public int Id { get; set; }
        public int WorkItemId { get; set; }
    }
}