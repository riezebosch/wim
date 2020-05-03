using System.Threading.Tasks;
using MigrateWorkItems.Tests.Data;

namespace MigrateWorkItems.Tests.FieldsProcessors
{
    internal class ReadOnlyFields : IFieldsProcessor
    {
        private static readonly string[] Fields =
        {
            "System.BoardColumn", 
            "System.BoardColumnDone",
            "System.ExternalLinkCount",
            "System.Parent",
            "System.RemoteLinkCount",
            "System.CommentCount",
            "System.HyperLinkCount",
            "System.AttachedFileCount",
            "System.NodeName",
            "System.Id",
            "System.Watermark",
            "System.Rev",
            "System.RelatedLinkCount",
            "System.BoardLane"
        };

        public Task Execute(WorkItemUpdate update)
        {
            if (update.Fields == null) return Task.CompletedTask;;
            foreach (var field in Fields)
            {
                RemoveReadOnlyField(update, field);
            }
            
            return Task.CompletedTask;
        }

        private static void RemoveReadOnlyField(WorkItemUpdate update, string field) => update.Fields.Remove(field);
    }
}