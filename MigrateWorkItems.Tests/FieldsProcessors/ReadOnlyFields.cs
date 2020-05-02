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
            "System.RevisedDate",
            "System.ChangedDate",
            "System.Id",
            "System.AuthorizedAs",
            "System.AuthorizedDate",
            "System.Watermark",
            "System.Rev",
            "System.CreatedDate",
            "System.CreatedBy",
            "System.RelatedLinkCount",
            "System.BoardLane"
        };

        public Task Execute(WorkItemUpdate update)
        {
            foreach (var field in Fields)
            {
                RemoveReadOnlyField(update, field);
            }
            
            return Task.CompletedTask;
        }

        private static void RemoveReadOnlyField(WorkItemUpdate update, string field) => update.Fields.Remove(field);
    }
}