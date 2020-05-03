using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MigrateWorkItems.Tests.Data;

namespace MigrateWorkItems.Tests.FieldsProcessors
{
    internal class ClassificationNodes : IFieldsProcessor
    {
        private readonly string _project;

        public ClassificationNodes(string project)
        {
            _project = project;
        }
        public Task Execute(WorkItemUpdate update)
        {
            if (update.Fields == null) return Task.CompletedTask;;
            ReplaceTeamProject(update, "System.AreaPath");
            ReplaceTeamProject(update, "System.IterationPath");

            update.Fields.Remove("System.TeamProject");
            update.Fields.Remove("System.AreaId");
            update.Fields.Remove("System.IterationId");
            
            return Task.CompletedTask;
        }

        private void ReplaceTeamProject(WorkItemUpdate first, string field)
        {
            if (first.Fields.TryGetValue(field, out var node))
            {
                first.Fields[field].NewValue = Regex.Replace((string)node.NewValue, @"^[^\\]*", _project);
            }
        }
    }
}