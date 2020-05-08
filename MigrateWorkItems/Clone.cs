using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureDevOpsRest;
using Flurl.Http;
using MigrateWorkItems.Data;
using MigrateWorkItems.Model;
using MigrateWorkItems.Save;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace MigrateWorkItems
{
    public static class Clone
    {
        public static async Task Run(string organization, string token, IEnumerable<string> areas, string output, Action<string> writeLine)
        {
            var dir = Directory.CreateDirectory(output);
            var client = new Client(token);
            await using var context = new MigrationContext(dir.FullName);
            await context.Database.EnsureCreatedAsync();

            try
            {
                writeLine("Downloading work item updates...");
                var download = new Download(client);
                var saveWorkItems = new SaveWorkItems(dir.CreateSubdirectory("items"));
                var saveAttachments = new SaveAttachments(client, dir.CreateSubdirectory("attachments"));
                
                await foreach (var update in download.To(organization, areas.ToArray()))
                {
                    saveWorkItems.To(update);
                    await Save(context, update);
                    
                    await foreach (var attachment in saveAttachments.To(update))
                    {
                        context.Attachments.Add(attachment);
                        await context.SaveChangesAsync();
                    }
                }
                
                // writeLine("Indexing updates...");
                // await WorkItemIndexer.Index(context, dir.CreateSubdirectory("items"));
                //
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

        private static async Task Save(MigrationContext context, JToken update)
        {
            var id = (int)update.SelectToken("id");
            var workItemId = (int)update.SelectToken("workItemId");
            
            if (context.Updates.Find(id, workItemId) == null)
            {
                await context.Updates.AddAsync(new Update
                {
                    Id = id,
                    WorkItemId = workItemId,
                    ChangeDate = update.ChangeDate(),
                    Relations = update.Relations()
                });
            }
        }

        public static WorkItemUpdate FromFile(string file) => Deserialize(File.ReadAllText(file));

        private static WorkItemUpdate Deserialize(string content) => 
            JsonConvert.DeserializeObject<WorkItemUpdate>(content, new JsonSerializerSettings {ContractResolver = new CamelCasePropertyNamesContractResolver()});
    }
}