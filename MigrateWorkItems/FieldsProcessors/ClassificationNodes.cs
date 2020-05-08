using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MigrateWorkItems.Data;

namespace MigrateWorkItems.FieldsProcessors
{
    public class ClassificationNodes : IFieldsProcessor
    {
        private readonly string _project;
        private readonly ICreateClassificationNodes _create;

        public ClassificationNodes(string project, ICreateClassificationNodes create)
        {
            _project = project;
            _create = create;
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
            if (first.Fields.TryGetValue(field, out var node))
            {
                var path = Regex.Replace((string)node.NewValue, @"^[^\\]*", _project);
                first.Fields[field].NewValue = path;
                
                CreateIfNotExists(field, path);
            }
        }

        private void CreateIfNotExists(string field, string value)
        {
            var path = value.Split('\\').Skip(1).ToArray();
            switch (field)
            {
                case "System.IterationPath":
                    _create.IfNotExists("iterations", path);
                    break;

                case "System.AreaPath":
                    _create.IfNotExists("areas", path);
                    break;
            }
        }
    }
}