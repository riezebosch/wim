using System.Diagnostics;
using System.Threading.Tasks;
using AzureDevOpsRest;
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

            WriteLine("Uploading attachments...");
            var start = Stopwatch.StartNew();
            
            await foreach (var (position, total) in AttachmentsProcessor.UploadAttachments(client, organization, project, context, output))
            {
                SetCursorPosition(0, CursorTop);
                Write($"[{position}/{total}] {(start.Elapsed / position * (total - position)).Humanize(2)} remaining");
            }

            WriteLine();
            WriteLine($"Done in {start.Elapsed.Humanize()}");
            
            try
            {
                start.Restart();
                WriteLine("Replaying operations...");
                
                var processor = new WorkItemProcessor(client, organization, project, new FieldsResolver(client, organization, project), new RelationsProcessors(client, mapper), mapper);
                await foreach (var (position, total) in Push.Run(organization, project, output, context, processor))
                {
                    SetCursorPosition(0, CursorTop);
                    Write($"[{position}/{total}] {(start.Elapsed / position * (total - position)).Humanize(2)} remaining");
                }
            }
            finally
            {
                await context.SaveChangesAsync();
            }

            WriteLine();
            WriteLine($"Done in {start.Elapsed.Humanize()}");
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
                
                cmd.OnExecuteAsync(async c =>
                {
                    WriteLine("Downloading work item updates and attachments...");
                    await foreach (var (totalItems, totalAttachments) in Clone.Run(organization.Value(), token.Value(), areaPaths.Values, output.Value()))
                    {
                        SetCursorPosition(0, CursorTop);
                        Write($"{totalItems} work item updates and {totalAttachments} attachments");
                    }
                });
            });
            
            
        }
    }
}