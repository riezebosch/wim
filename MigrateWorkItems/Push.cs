using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flurl.Http;
using MigrateWorkItems.Model;

namespace MigrateWorkItems
{
    public static class Push
    {
        public static async IAsyncEnumerable<(int position, int total)> Run(string organization, string project, string output, MigrationContext context, WorkItemProcessor processor)
        {
            var query = context
                .Updates
                .AsQueryable()
                .Where(x => !x.Done)
                .OrderBy(x => x.ChangeDate)
                .ThenByDescending(x => x.RelationsAdded + x.RelationsRemoved)
                .ThenByDescending(x => x.RelationsRemoved);

            var position = 0;
            var total = query.Count();
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
                    System.Console.WriteLine(ex.Call.Request.RequestUri.ToString());
                    System.Console.WriteLine(ex.Call.RequestBody);

                    if (ex.Call.Response?.Content != null)
                    {
                        System.Console.WriteLine(await ex.Call.Response.Content.ReadAsStringAsync());
                    }

                    throw;
                }

                await context.SaveChangesAsync();
                yield return (++position, total);
            }
        }
    }
}