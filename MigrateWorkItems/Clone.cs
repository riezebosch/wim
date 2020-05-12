using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureDevOpsRest;
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
        public static async IAsyncEnumerable<(int totalItems, int totalAttachments)> Run(string organization,
            string token, IEnumerable<string> areas, string output)
        {
            var dir = Directory.CreateDirectory(output);
            var client = new Client(token);
            await using var context = new MigrationContext(dir.FullName);
            await context.Database.EnsureCreatedAsync();

            var download = new Download(client);
            var saveWorkItems = new SaveWorkItems(dir.CreateSubdirectory("items"));
            var saveAttachments = new SaveAttachments(client, dir.CreateSubdirectory("attachments"));

            var totalItems = 0;
            var totalAttachments = 0;
            await foreach (var update in download.To(organization, areas.ToArray()))
            {
                saveWorkItems.To(update);
                await Save(context, update);

                await foreach (var attachment in saveAttachments.To(update))
                {
                    context.Attachments.Add(new AttachmentMapping { Id = attachment.Id});
                    totalAttachments++;
                }

                totalItems++;

                if (totalItems % 1000 == 0)
                {
                    await context.SaveChangesAsync();
                }
            
                yield return (totalItems, totalAttachments);
            }

            await context.SaveChangesAsync();
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
                    RelationsAdded = update.RelationsAdded(),
                    RelationsRemoved = update.RelationsRemoved()
                });
            }
        }

        public static WorkItemUpdate FromFile(string file) => Deserialize(File.ReadAllText(file));

        private static WorkItemUpdate Deserialize(string content) => 
            JsonConvert.DeserializeObject<WorkItemUpdate>(content, new JsonSerializerSettings {ContractResolver = new CamelCasePropertyNamesContractResolver()});
    }
}