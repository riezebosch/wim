using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureDevOpsRest;
using Flurl.Http;
using Humanizer;
using McMaster.Extensions.CommandLineUtils;
using MigrateWorkItems.Model;
using MigrateWorkItems.Relations;
using static System.Console;

namespace MigrateWorkItems.Console
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.HelpOption();
            
            AddCloneCommand(app);
            AddPush(app);
            
            app.OnExecuteAsync(cancel => Task.CompletedTask);
            await app.ExecuteAsync(args);
        }

        private static void AddPush(CommandLineApplication app)
        {
            app.Command("push", cmd =>
            {
                var organization = cmd.Option("--organization", "",
                    CommandOptionType.SingleValue).IsRequired();
                
                var project = cmd.Option("-p|--project", "",
                    CommandOptionType.SingleValue).IsRequired();
                
                var token = cmd.Option("-t|--token", "",
                    CommandOptionType.SingleValue).IsRequired();
                
                var output = cmd.Option("-o|--output", "",
                    CommandOptionType.SingleValue).IsRequired();
                
                cmd.HelpOption();
                
                cmd.OnExecuteAsync(c => RunPush(organization.Value(), token.Value(), project.Value(), output.Value()));

            });
        }

        private static async Task RunPush(string organization, string token, string project, string output)
        {
            var client = new Client(token);

            await using var context = new MigrationContext(output);
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            
            var mapper = new Mapper(context);

            WriteLine("Uploading attachments");
            await AttachmentsProcessor.UploadAttachments(client, organization, project, context, output);

            var processor = new WorkItemProcessor(client, organization, project, new FieldsResolver(client, organization, project), new RelationsProcessors(client, mapper), mapper);

            var i = 0;
            try
            {
                var query = context
                    .Updates
                    .AsQueryable()
                    .Where(x => !x.Done)
                    .OrderBy(x => x.ChangeDate)
                    .ThenByDescending(x => x.Relations);

                var total = query.Count();
                var start = Stopwatch.StartNew();
                
                WriteLine("Performing work item updates...");
                foreach (var item in query)
                {
                    var update = Clone.FromFile(Path.Join(output, "items", item.WorkItemId.ToString(), item.Id + ".json"));

                    try
                    {
                        await processor.Process(organization, project, update);
                        item.Done = true;
                        context.Updates.Update(item);
                    }
                    catch (FlurlHttpException ex)
                    {
                        WriteLine(ex.Call.Request.RequestUri.ToString());
                        WriteLine(ex.Call.RequestBody);

                        if (ex.Call.Response?.Content != null)
                        {
                            WriteLine(await ex.Call.Response.Content.ReadAsStringAsync());
                        }

                        throw;
                    }

                    await context.SaveChangesAsync();
                    
                    SetCursorPosition(0, CursorTop);
                    Write($"[{++i}/{total}] {(start.Elapsed / i * total).Humanize()} remaining");
                }
            }
            finally
            {
                await context.SaveChangesAsync();
            }
        }

        private static void AddCloneCommand(CommandLineApplication app)
        {
            app.Command("clone", cmd =>
            {
                var organization = cmd.Option("--organization", "",
                    CommandOptionType.SingleValue).IsRequired();

                var areaPaths = cmd.Option("-a|--area-path", "",
                    CommandOptionType.MultipleValue).IsRequired();

                var token = cmd.Option("-t|--token", "",
                    CommandOptionType.SingleValue).IsRequired();
                
                var output = cmd.Option("-o|--output", "",
                    CommandOptionType.SingleValue).IsRequired();

                cmd.HelpOption();
                
                cmd.OnExecuteAsync(c => Clone.RunClone(organization.Value(), token.Value(), areaPaths.Values, output.Value(), WriteLine));
            });
            
            
        }
    }
}