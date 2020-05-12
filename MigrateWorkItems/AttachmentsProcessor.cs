using System.Collections.Generic;
using System.IO;
using System.Linq;
using AzureDevOpsRest;
using AzureDevOpsRest.Requests;
using MigrateWorkItems.Data;
using MigrateWorkItems.Model;

namespace MigrateWorkItems
{
    public static class AttachmentsProcessor
    {
        public static async IAsyncEnumerable<(int, int total)> UploadAttachments(IClient client, string organization, string project,
            MigrationContext context, string output)
        {
            var position = 0;
            var query = context
                .Attachments
                .AsQueryable()
                .Where(x => x.Url == null);

            var total = query.Count();
            foreach (var attachment in query)
            {
                await using var stream = File.OpenRead(Path.Join(output, "attachments", attachment.Id.ToString()));
                var result = await client.PostAsync(
                    new Request<AttachmentReference>($"{organization}/{project}/_apis/wit/attachments", "5.1")
                        .WithQueryParams(("fileName", attachment.FileName)),
                    stream);

                attachment.Url = result.Url;
                context.Attachments.Update(attachment);

                await context.SaveChangesAsync();
                yield return (++position, total);
            }
        }
    }
}