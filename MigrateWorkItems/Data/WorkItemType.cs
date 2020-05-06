using System.Collections.Generic;

namespace MigrateWorkItems.Data
{
    public class WorkItemType
    {
        public IEnumerable<WorkItemField> Fields { get; set; }
    }
}