using System.Text.RegularExpressions;
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
        public void Execute(WorkItemUpdate update)
        {
            ReplaceTeamProject(update, "System.AreaPath");
            ReplaceTeamProject(update, "System.IterationPath");

            update.Fields.Remove("System.TeamProject");
        }

        private void ReplaceTeamProject(WorkItemUpdate first, string field)
        {
            if (first.Fields.TryGetValue(field, out var node))
            {
                first.Fields[field].NewValue =
                    Regex.Replace((string)node.NewValue, @"^[^\\]*", _project);
            }
        }
    }
}