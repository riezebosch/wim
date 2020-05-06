using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureDevOpsRest;
using Flurl.Http;
using MigrateWorkItems.Index;
using MigrateWorkItems.Model;
using Newtonsoft.Json.Linq;

namespace MigrateWorkItems
{
    public static class Clone
    {
        public static async Task RunClone(string organization, string token, IEnumerable<string> areas, string output, Action<string> writeLine)
        {
            var client = new Client(token);
            var dir = Directory.CreateDirectory(output);
            var items = dir.CreateSubdirectory("items");
            var attachments = dir.CreateSubdirectory("attachments");
            
            try
            {
                writeLine("Downloading work item updates...");
                var save = new SaveWorkItems(client);
                var updates = new List<JToken>();
                await foreach (var item in save.To(items, organization, attachments,
                    areas.ToArray()))
                {
                    await foreach (var update in item)
                    {
                        updates.AddRange(update);
                    }
                }
                
                await using var context = new MigrationContext(dir.FullName);
                await context.Database.EnsureCreatedAsync();
                
                writeLine("Downloading attachments...");
                var save2 = new SaveAttachments(client);
                await foreach (var attachment in save2.To(attachments, updates))
                {
                    context.Attachments.Add(attachment);
                    await context.SaveChangesAsync();
                }
                
                
                
                writeLine("Indexing updates...");
                await WorkItemIndexer.Index(context, items);
                
                // WriteLine("Indexing attachments...");
                // await AttachmentIndexer.Index(context, attachments);
            }
            catch (FlurlHttpException ex)
            {
                writeLine(ex.Call.Request.RequestUri.ToString());
                writeLine(ex.Call.RequestBody);

                if (ex.Call.Response?.Content != null)
                {
                    writeLine(await ex.Call.Response.Content.ReadAsStringAsync());
                }

                throw;
            }
        }
    }
}