using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MigrateWorkItems.Data;
using MigrateWorkItems.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MigrateWorkItems
{
    public static class Indexer
    {
        public static WorkItemUpdate FromFile(string file) 
            =>
                Deserialize(File.ReadAllText(file));

        private static WorkItemUpdate Deserialize(string content) => 
            JsonConvert.DeserializeObject<WorkItemUpdate>(content, new JsonSerializerSettings {ContractResolver = new CamelCasePropertyNamesContractResolver()});

        public static async Task Index(MigrationContext context, DirectoryInfo items)
        {
            var updates =
                items.EnumerateDirectories()
                    .SelectMany(x => x.EnumerateFiles());

            var i = 0;
            foreach (var file in updates.AsParallel())
            {
                var update = FromFile(file.FullName);
                await context.Updates.AddAsync(new Model.Update
                {
                    WorkItemId = update.WorkItemId,
                    Id = update.Id,
                    ChangeDate = (DateTime?) update.Fields?["System.ChangedDate"].NewValue
                                 ?? (DateTime?) update.Fields?["System.CreatedDate"].NewValue
                                 ?? update.RevisedDate,
                    Relations = update.Relations?.Added?.Count() + update.Relations?.Removed?.Count() ?? 0
                });

                if (i++ % 1000 == 0)
                {
                    await context.SaveChangesAsync();
                }
            }

            await context.SaveChangesAsync();
        }
    }
}