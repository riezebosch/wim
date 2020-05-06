using System.Threading.Tasks;
using MigrateWorkItems.Data;

namespace MigrateWorkItems.FieldsProcessors
{
    internal class ClassificationNodesResetToTeamProject : IFieldsProcessor
    {
        private readonly string _project;

        public ClassificationNodesResetToTeamProject(string project)
        {
            _project = project;
        }
        public Task Execute(WorkItemUpdate update)
        {
            if (update.Fields == null) return Task.CompletedTask;
            ReplaceTeamProject(update, "System.AreaPath");
            ReplaceTeamProject(update, "System.IterationPath");

            update.Fields.Remove("System.TeamProject");
            update.Fields.Remove("System.AreaId");
            update.Fields.Remove("System.IterationId");
            
            return Task.CompletedTask;
        }

        private void ReplaceTeamProject(WorkItemUpdate first, string field)
        {
            if (first.Fields.ContainsKey(field))
            {
                first.Fields[field].NewValue = _project;
            }
        }
    }
}