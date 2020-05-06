using System;
using System.IO;
using System.Threading.Tasks;
using MigrateWorkItems.Model;

namespace MigrateWorkItems.Index
{
    public static class AttachmentIndexer
    {
        public static async Task Index(MigrationContext context, DirectoryInfo attachments)
        {
            foreach (var file in attachments
                .EnumerateFiles("*.*"))
            {
                var id = new Guid(file.Name);
                if (context.Attachments.Find(id) == null)
                {
                    context.Attachments.Add(new AttachmentMapping { Id = id });
                }
            }
            
            await context.SaveChangesAsync();
        }
    }
}