using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AzureDevOpsRest;
using MigrateWorkItems.Model;
using Newtonsoft.Json.Linq;

namespace MigrateWorkItems
{
    public interface ISaveAttachments
    {
        Task<AttachmentMapping> To(DirectoryInfo attachments, JToken update);
    }

    public class SaveAttachments : ISaveAttachments
    {
        private static readonly object Mutex = new object();
        private readonly IClient _client;

        public SaveAttachments(IClient client) => _client = client;


        public async Task<AttachmentMapping> To(DirectoryInfo attachments, JToken update)
        {
            var description = (string) update.SelectToken("fields.['System.Description'].newValue");
            if (description != null && description.TryGetAttachmentUrl(out var url, out var id, out var filename))
            {
                var path = Path.Join(attachments.FullName, id.ToString());
                lock(Mutex)
                {
                    if (File.Exists(path))
                    {
                        return new AttachmentMapping {Id = id, FileName = filename};
                    }
                }

                await using var stream = await _client.GetAsync(new UriRequest<Stream>(url, "5.1"));
                await stream.CopyToAsync(File.Create(path));

                return new AttachmentMapping {Id = id, FileName = filename};
            }

            return null;
        }

        public async IAsyncEnumerable<AttachmentMapping> To(DirectoryInfo attachments, IEnumerable<JToken> updates)
        {
            foreach (var update in updates)
            {
                var attachment = await To(attachments, update);
                if (attachment != null)
                {
                    yield return attachment;
                }
            }
        }
    }
}