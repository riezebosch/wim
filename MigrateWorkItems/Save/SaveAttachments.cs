using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AzureDevOpsRest;
using MigrateWorkItems.Data;
using Newtonsoft.Json.Linq;

namespace MigrateWorkItems.Save
{
    public interface ISaveAttachments
    {
        IAsyncEnumerable<AttachmentReference> To(JToken update);
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


        public async IAsyncEnumerable<AttachmentReference> To(JToken update)
        {
            foreach (var attachment in FindAttachments(update))
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

        private static IEnumerable<AttachmentReference> FindAttachments(JToken update)
        {
            var description = (string) update.SelectToken("fields.['System.Description'].newValue");
            if (description != null)
            {
                foreach (var attachment in description.GetAttachments())
                {
                    yield return attachment;
                }
            }
            
            var relations = update.SelectTokens("relations.added[?(@.rel=='AttachedFile')].url").Values<string>();
            foreach (var uri in relations.Select(x => new Uri(x)))
            {
                yield return new AttachmentReference
                {
                    Id = new Guid(uri.Segments.Last()),
                    Url = uri
                };
            }
        }
    }
}