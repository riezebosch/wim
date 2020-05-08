using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureDevOpsRest;
using AzureDevOpsRest.Requests;
using MigrateWorkItems.Data;
using MigrateWorkItems.Model;

namespace MigrateWorkItems
{
    public static class AttachmentsProcessor
    {
        public static async Task UploadAttachments(IClient client, string organization, string project,
            MigrationContext context, string output)
        {
            foreach (var attachment in context
                .Attachments
                .AsQueryable()
                .Where(x => x.Url == null))
            {
                await using var stream = File.OpenRead(Path.Join(output, "attachments", attachment.Id.ToString()));
                var result = await client.PostAsync(
                    new Request<AttachmentReference>($"{organization}/{project}/_apis/wit/attachments", "5.1")
                        .WithQueryParams(("fileName", attachment.FileName)),
                    stream);

                attachment.Url = result.Url;
                context.Attachments.Update(attachment);

                await context.SaveChangesAsync();
            }
        }
    }
}