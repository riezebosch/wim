using System.Collections.Generic;
using System.IO;
using AzureDevOpsRest;
using MigrateWorkItems.Model;
using Newtonsoft.Json.Linq;

namespace MigrateWorkItems.Save
{
    public interface ISaveAttachments
    {
        IAsyncEnumerable<AttachmentMapping> To(JToken update);
    }

    public class SaveAttachments : ISaveAttachments
    {
        private static readonly object Mutex = new object();
        private readonly IClient _client;
        private readonly DirectoryInfo _target;

        public SaveAttachments(IClient client, DirectoryInfo target)
        {
            _client = client;
            _target = target;
        }


        public async IAsyncEnumerable<AttachmentMapping> To(JToken update)
        {
            var description = (string) update.SelectToken("fields.['System.Description'].newValue");
            if (description != null)
            {
                foreach (var attachment in description.GetAttachments())
                {
                    var path = Path.Join(_target.FullName, attachment.Id.ToString());
                    lock (Mutex)
                    {
                        if (File.Exists(path))
                        {
                            continue;
                        }
                    }

                    await using var stream = await _client.GetAsync(new UriRequest<Stream>(attachment.Url, "5.1"));
                    await stream.CopyToAsync(File.Create(path));

                    yield return attachment;
                }
            }
        }
    }
}